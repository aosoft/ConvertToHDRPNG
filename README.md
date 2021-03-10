Convert To HDR PNG
====

## About

This application converts a still image (EXR) to a 16-bit PNG in BT.2020/ST.2084 color space. It is used to generate intermediate sources for conversion to AVIF.

## How to use

Required: .NET 5.0 or later

### Convert Image

```
$ dotnet run convert --input src.exr --output dst.png
```

### Generate Gradation Image

```
$ dotnet run -- gradation -h
$ dotnet run grdation --output dst.png --start 0 --end 2
$ dotnet run grdation --output dst.exr --start 0 --end 2
```

If the extension of the output file is .exr, it will be output as scRGB EXR. Otherwise, the output file will be a PNG with the color space converted to BT.2020/ST.2084.

## Dependencies

* [Magick.NET](https://github.com/dlemstra/Magick.NET)  - Apatch License 2.0 ([LICENSE](https://github.com/dlemstra/Magick.NET/blob/main/License.txt))
* [ImageMagick](https://imagemagick.org/) - Apatch License 2.0 ([LICENSE](https://github.com/ImageMagick/ImageMagick/blob/main/LICENSE))
* [Cocona](https://github.com/mayuki/Cocona) - MIT ([LICENSE](https://github.com/mayuki/Cocona/blob/master/LICENSE))

## References

* [color.hlsli](https://github.com/microsoft/DirectX-Graphics-Samples/blob/master/Samples/Desktop/D3D12HDR/src/color.hlsli) - MIT

