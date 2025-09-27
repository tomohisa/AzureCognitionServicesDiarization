#!/bin/bash

# Azure Speech Services 話者分離デモ実行スクリプト（macOS/Linux用）

echo "Azure Speech Services 話者分離デモを開始します..."
echo

# 環境変数の確認
if [ -z "$SPEECH_KEY" ]; then
    echo "❌ エラー: SPEECH_KEY 環境変数が設定されていません"
    echo "以下のコマンドで設定してください:"
    echo "export SPEECH_KEY=your_speech_key_here"
    echo "export SPEECH_REGION=japaneast"
    echo
    exit 1
fi

if [ -z "$SPEECH_REGION" ]; then
    echo "❌ エラー: SPEECH_REGION 環境変数が設定されていません"
    echo "以下のコマンドで設定してください:"
    echo "export SPEECH_KEY=your_speech_key_here"
    echo "export SPEECH_REGION=japaneast"
    echo
    exit 1
fi

echo "✅ 環境変数が設定されています"
echo "リージョン: $SPEECH_REGION"
echo

# .NET 10 シングルファイルアプリケーションの実行
dotnet run --file AzureSpeechDiarization.cs