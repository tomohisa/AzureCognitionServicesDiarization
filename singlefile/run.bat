@echo off
REM Azure Speech Services 話者分離デモ実行スクリプト（Windows用）

echo Azure Speech Services 話者分離デモを開始します...
echo.

REM 環境変数の確認
if "%SPEECH_KEY%"=="" (
    echo ❌ エラー: SPEECH_KEY 環境変数が設定されていません
    echo 以下のコマンドで設定してください:
    echo set SPEECH_KEY=your_speech_key_here
    echo set SPEECH_REGION=japaneast
    echo.
    pause
    exit /b 1
)

if "%SPEECH_REGION%"=="" (
    echo ❌ エラー: SPEECH_REGION 環境変数が設定されていません
    echo 以下のコマンドで設定してください:
    echo set SPEECH_KEY=your_speech_key_here
    echo set SPEECH_REGION=japaneast
    echo.
    pause
    exit /b 1
)

echo ✅ 環境変数が設定されています
echo リージョン: %SPEECH_REGION%
echo.

REM .NET 10 シングルファイルアプリケーションの実行
dotnet run --file AzureSpeechDiarization.cs

pause