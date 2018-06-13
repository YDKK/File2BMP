# File2BMP
Convert any binary file to valid Bitmap file.  
https://ydkk.hateblo.jp/entry/2016/01/25/115224

<img src="https://cdn-ak.f.st-hatena.com/images/fotolife/Y/YDKK/20180614/20180614024112.png">

## Usage
Convert to BMP:
```
> File2BMP.exe hoge.bin
```

Restore from BMP:
```
> File2BMP.exe hoge.bin.bmp
```

## Note
This program will use `bfReserved1` and `bfReserved2` in the Bitmap Fie Header if convert source file size is larger than 4.3GB (`uint.MaxValue`).  
So now, we can support up to 9.2EB! (Maximum supported size of `Stream`.)

Please note that this is not valid Bitmap file.


Enjoy!
