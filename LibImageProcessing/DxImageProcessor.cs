using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LibImageProcessing.Extensions;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace LibImageProcessing
{
    public unsafe abstract class DxImageProcessor : IImageProcessor
    {
        private static D3D11? _api;
        private static D3DCompiler? _compiler;
        private static volatile int _instanceCount;

        ComPtr<ID3D11Device> _device;
        ComPtr<ID3D11DeviceContext> _deviceContext;
        ComPtr<ID3D11Texture2D> _renderTarget;
        ComPtr<ID3D11Texture2D> _outputBuffer;
        ComPtr<ID3D11Buffer> _vertexBuffer;
        ComPtr<ID3D11Buffer> _indexBuffer;
        ComPtr<ID3D10Blob> _vertexShaderCode;
        ComPtr<ID3D10Blob> _pixelShaderCode;
        ComPtr<ID3D11VertexShader> _vertexShader;
        ComPtr<ID3D11PixelShader> _pixelShader;
        ComPtr<ID3D11InputLayout> _inputLayout;

        private int _inputWidth;
        private int _inputHeight;
        private float[]? _background;
        private bool _disposedValue;

        public int InputWidth => _inputWidth;
        public int InputHeight => _inputHeight;
        public abstract int OutputWidth { get; }
        public abstract int OutputHeight { get; }

        public DxImageProcessor(
            int inputWidth, int inputHeight)
        {
            _instanceCount++;
            _inputWidth = inputWidth;
            _inputHeight = inputHeight;
        }

        protected internal abstract string GetShaderCode();

        [MemberNotNull(nameof(_api))]
        [MemberNotNull(nameof(_compiler))]
        [MemberNotNull(nameof(_background))]
        private void EnsureInitialized()
        {
            if (_api is not null &&
                _compiler is not null &&
                _background is not null &&
                _device.Handle is not null &&
                _deviceContext.Handle is not null)
            {
                return;
            }

            _api ??= D3D11.GetApi(null, false);
            _compiler ??= D3DCompiler.GetApi();

            _api.CreateDevice(ref Unsafe.NullRef<IDXGIAdapter>(), D3DDriverType.Hardware, 0, (uint)CreateDeviceFlag.Debug, ref Unsafe.NullRef<D3DFeatureLevel>(), 0, D3D11.SdkVersion, ref _device, null, ref _deviceContext);

            var renderTargetDesc = new Texture2DDesc()
            {
                Width = (uint)OutputWidth,
                Height = (uint)OutputHeight,
                ArraySize = 1,
                BindFlags = (uint)BindFlag.RenderTarget,
                CPUAccessFlags = 0,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Default,
            };

            var outputBufferDesc = new Texture2DDesc()
            {
                Width = (uint)OutputWidth,
                Height = (uint)OutputHeight,
                ArraySize = 1,
                BindFlags = 0,
                CPUAccessFlags = (uint)CpuAccessFlag.Read,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Staging,
            };

            _device.CreateTexture2D(in renderTargetDesc, ref Unsafe.NullRef<SubresourceData>(), ref _renderTarget);
            _device.CreateTexture2D(in outputBufferDesc, ref Unsafe.NullRef<SubresourceData>(), ref _outputBuffer);

            var vertices = new float[]
            {
                -1, -1, 0,
                -1, 1, 0,
                1, 1, 0,
                1, -1, 0
            };

            var indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3
            };

            fixed (float* vertexData = vertices)
            {
                BufferDesc bufferDesc = new BufferDesc
                {
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    ByteWidth = (uint)(sizeof(float) * vertices.Length),
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    Usage = Usage.Default,
                };

                SubresourceData data = new SubresourceData
                {
                    PSysMem = vertexData,
                };

                _device.CreateBuffer(in bufferDesc, in data, ref _vertexBuffer);
            }

            fixed (uint* indexData = indices)
            {
                BufferDesc bufferDesc = new BufferDesc
                {
                    BindFlags = (uint)BindFlag.IndexBuffer,
                    ByteWidth = (uint)(sizeof(uint) * indices.Length),
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    Usage = Usage.Default,
                };

                SubresourceData data = new SubresourceData
                {
                    PSysMem = indexData,
                };

                _device.CreateBuffer(in bufferDesc, in data, ref _indexBuffer);
            }

            string shaderCode = GetShaderCode();
            byte[] shaderCodeBytes = Encoding.ASCII.GetBytes(shaderCode);

            fixed (byte* pShaderCode = shaderCodeBytes)
            {
                ComPtr<ID3D10Blob> errorMsgs = null;
                _compiler.Compile(pShaderCode, (nuint)(shaderCodeBytes.Length), "shader", ref Unsafe.NullRef<D3DShaderMacro>(), ref Unsafe.NullRef<ID3DInclude>(), "vs_main", "vs_5_0", 0, 0, ref _vertexShaderCode, ref errorMsgs);
                if (errorMsgs.Handle != null)
                {
                    string error = Encoding.ASCII.GetString((byte*)errorMsgs.GetBufferPointer(), (int)errorMsgs.GetBufferSize());
                    throw new InvalidOperationException(error);
                }

                _compiler.Compile(pShaderCode, (nuint)(shaderCodeBytes.Length), "shader", ref Unsafe.NullRef<D3DShaderMacro>(), ref Unsafe.NullRef<ID3DInclude>(), "ps_main", "ps_5_0", 0, 0, ref _pixelShaderCode, ref errorMsgs);
                if (errorMsgs.Handle != null)
                {
                    string error = Encoding.ASCII.GetString((byte*)errorMsgs.GetBufferPointer(), (int)errorMsgs.GetBufferSize());
                    throw new InvalidOperationException(error);
                }

                _device.CreateVertexShader(_vertexShaderCode.GetBufferPointer(), _vertexShaderCode.GetBufferSize(), ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _vertexShader);
                _device.CreatePixelShader(_pixelShaderCode.GetBufferPointer(), _pixelShaderCode.GetBufferSize(), ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _pixelShader);
            }

            var sematicName = "POSITION";
            var sematicNameBytes = Encoding.ASCII.GetBytes(sematicName);

            fixed (byte* pSematicName = sematicNameBytes)
            {
                InputElementDesc inputElementDesc = new InputElementDesc
                {
                    SemanticName = (byte*)pSematicName,
                    SemanticIndex = 0,
                    Format = Format.FormatR32G32B32Float,
                    InputSlot = 0,
                    InputSlotClass = InputClassification.PerVertexData,
                };

                _device.CreateInputLayout(in inputElementDesc, 1, _vertexShaderCode.GetBufferPointer(), _vertexShaderCode.GetBufferSize(), ref _inputLayout);
            }

            _background = new float[]
            {
                0,
                0,
                0,
                1.0f
            };
        }

        public void Process(ReadOnlySpan<byte> bgraInput, Span<byte> bgraOutput)
        {
            EnsureInitialized();

            if (bgraInput.Length != (InputWidth * InputHeight * 4))
            {
                throw new ArgumentException("Size not match", nameof(bgraInput));
            }

            if (bgraOutput.Length != (OutputWidth * OutputHeight * 4))
            {
                throw new ArgumentException("Size not match", nameof(bgraOutput));
            }

            var inputTextureDesc = new Texture2DDesc()
            {
                Width = (uint)InputWidth,
                Height = (uint)InputHeight,
                ArraySize = 1,
                BindFlags = (uint)BindFlag.ShaderResource,
                CPUAccessFlags = 0,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Immutable,
            };

            ComPtr<ID3D11Texture2D> inputTexture = default;

            fixed (byte* ptr = bgraInput)
            {
                SubresourceData subresourceData = new SubresourceData
                {
                    PSysMem = ptr,
                    SysMemPitch = (uint)(InputWidth * 4),
                    SysMemSlicePitch = (uint)(InputWidth * InputHeight * 4)
                };

                var hr = _device.CreateTexture2D(in inputTextureDesc, in subresourceData, ref inputTexture);
                SilkMarshal.ThrowHResult(hr);

                ShaderResourceViewDesc inputTextureShaderResourceViewDesc = new ShaderResourceViewDesc
                {
                    Format = inputTextureDesc.Format,
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionTexture2D,
                };

                inputTextureShaderResourceViewDesc.Texture2D.MipLevels = 1;
                inputTextureShaderResourceViewDesc.Texture2D.MostDetailedMip = 0;

                ComPtr<ID3D11ShaderResourceView> inputTextureShaderResourceView = default;
                _device.CreateShaderResourceView(inputTexture, in inputTextureShaderResourceViewDesc, ref inputTextureShaderResourceView);

                SamplerDesc samplerDesc = new SamplerDesc
                {
                    Filter = Filter.MinMagMipPoint,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                };

                ComPtr<ID3D11SamplerState> samplerState = default;
                _device.CreateSamplerState(in samplerDesc, ref samplerState);

                var viewport = new Viewport(0, 0, OutputWidth, OutputHeight, 0, 1);

                ComPtr<ID3D11RenderTargetView> renderTargetView = default;
                _device.CreateRenderTargetView<ID3D11Texture2D, ID3D11RenderTargetView>(_renderTarget, in Unsafe.NullRef<RenderTargetViewDesc>(), ref renderTargetView);

                // clear ouptut
                _deviceContext.ClearRenderTargetView(renderTargetView, ref _background[0]);

                _deviceContext.RSSetViewports(1, in viewport);
                _deviceContext.OMSetRenderTargets(1, ref renderTargetView, ref Unsafe.NullRef<ID3D11DepthStencilView>());

                _deviceContext.VSSetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
                _deviceContext.PSSetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);

                _deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
                _deviceContext.IASetInputLayout(_inputLayout);

                uint vertexStride = sizeof(float) * 3;
                uint vertexOffset = 0;
                _deviceContext.IASetVertexBuffers(0, 1, ref _vertexBuffer, in vertexStride, in vertexOffset);
                _deviceContext.IASetIndexBuffer(_indexBuffer, Format.FormatR32Uint, 0);

                _deviceContext.PSSetShaderResources(0, 1, ref inputTextureShaderResourceView);
                _deviceContext.PSSetSamplers(0, 1, ref samplerState);

                _deviceContext.DrawIndexed(6, 0, 0);

                _deviceContext.CopyResource(_outputBuffer, _renderTarget);

                MappedSubresource mappedSubresource = default;
                _deviceContext.Map(_outputBuffer, 0, Map.Read, 0, ref mappedSubresource);

                fixed (byte* outputPtr = bgraOutput)
                {
                    for (int y = 0; y < OutputHeight; y++)
                    {
                        var lineBytes = OutputWidth * 4;
                        NativeMemory.Copy((byte*)((nint)mappedSubresource.PData + mappedSubresource.RowPitch * y), outputPtr + lineBytes * y, (nuint)lineBytes);
                    }
                }

                _deviceContext.Unmap(_outputBuffer, 0);

                renderTargetView.Dispose();
                inputTextureShaderResourceView.Dispose();
                samplerState.Dispose();
                inputTexture.Dispose();
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~DxImageProcessor()
        {
            _instanceCount--;
            Dispose();
        }

        public void Dispose()
        {
            _device.DisposeIfNotNull();
            _deviceContext.DisposeIfNotNull();
            _renderTarget.DisposeIfNotNull();
            _outputBuffer.DisposeIfNotNull();
            _vertexBuffer.DisposeIfNotNull();
            _indexBuffer.DisposeIfNotNull();
            //_vertexShaderCode.DisposeIfNotNull();
            //_pixelShaderCode.DisposeIfNotNull();
            _vertexShader.DisposeIfNotNull();
            _pixelShader.DisposeIfNotNull();
            _inputLayout.DisposeIfNotNull();

            if (_instanceCount == 0)
            {
                _api?.Dispose();
                _compiler?.Dispose();

                _api = null;
                _compiler = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
