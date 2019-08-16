# PS1C

**PS1C** is a simple Zipfile Reader

### Working Features
`dir`
- Reads Directory Structure

`Get-Content`
- Reads content of files

### TODO
- Add Write Capability
- Better Stream manipulation.


### Example Usage

```bash
import-module "path\PS1C.dll"
new-psdrive -name APPX -psprovider PS1C -root "$($pwd.path)\ZipFile.zip"
cd APPX:
ls | ft Name, FullName, *dir*

get-content APPX:\file.txt -raw
```

_See_ [_*Example Scripts*_](https://github.com/romero126/PS1C/blob/master/Example%20Scripts.txt) _for more usage data._
