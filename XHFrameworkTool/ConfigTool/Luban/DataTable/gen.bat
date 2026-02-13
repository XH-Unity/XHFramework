  set WORKSPACE=../../../..
  set GEN_CLIENT=../Luban/Luban.dll
  set CUSTOM_TEMPLATE=../Luban/Templates

  dotnet %GEN_CLIENT% ^
      -t client ^
      -c cs-bin ^
      -d bin ^
      --conf ./luban.conf ^
      --customTemplateDir %CUSTOM_TEMPLATE%\cs-bin ^
      -x outputDataDir=..\..\..\..\XHFrameworkClient\Assets\XHFramework.Game\PackageAssets\DataTable ^
      -x outputCodeDir=..\..\..\..\XHFrameworkClient\Assets\XHFramework.Game\HotUpdateScripts\DataTable ^
      -x tableImporter.valueTypeNameFormat=Table{0}

  pause                              
