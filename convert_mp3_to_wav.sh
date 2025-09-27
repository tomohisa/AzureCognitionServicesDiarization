#!/bin/bash

# MP3 to WAV Converter for Azure Speech Services
# This script converts MP3 files to WAV format optimized for speech recognition

echo "🎵 MP3 to WAV Converter for Azure Speech Services"
echo "================================================="
echo

# Check if ffmpeg is installed
if ! command -v ffmpeg &> /dev/null; then
    echo "❌ エラー: ffmpeg がインストールされていません"
    echo
    echo "macOS での ffmpeg インストール方法:"
    echo "brew install ffmpeg"
    echo
    echo "Ubuntu/Debian での ffmpeg インストール方法:"
    echo "sudo apt update && sudo apt install ffmpeg"
    echo
    echo "Windows での ffmpeg インストール方法:"
    echo "https://ffmpeg.org/download.html からダウンロードしてPATHに追加"
    exit 1
fi

# Function to convert a single file
convert_file() {
    local input_file="$1"
    local output_file="$2"

    echo "🔄 変換中: $(basename "$input_file") → $(basename "$output_file")"

    # Convert MP3 to WAV with optimal settings for Azure Speech Services
    ffmpeg -i "$input_file" \
           -ar 16000 \
           -ac 1 \
           -f wav \
           -y \
           "$output_file" \
           -loglevel warning

    if [ $? -eq 0 ]; then
        echo "✅ 変換完了: $(basename "$output_file")"
        echo "   ファイルサイズ: $(ls -lh "$output_file" | awk '{print $5}')"

        # Calculate duration
        local duration=$(ffprobe -v quiet -show_entries format=duration -of csv=p=0 "$output_file" 2>/dev/null)
        if [ ! -z "$duration" ]; then
            local minutes=$(echo "$duration / 60" | bc -l | xargs printf "%.1f")
            echo "   音声長: ${minutes}分"
        fi
        echo
        return 0
    else
        echo "❌ 変換失敗: $(basename "$input_file")"
        echo
        return 1
    fi
}

# Check command line arguments
if [ $# -eq 0 ]; then
    echo "使用方法:"
    echo "  $0 <MP3ファイル> [出力WAVファイル]"
    echo "  $0 *.mp3                        # 複数ファイル一括変換"
    echo
    echo "例:"
    echo "  $0 audio.mp3                    # audio.wav として出力"
    echo "  $0 audio.mp3 output.wav         # output.wav として出力"
    echo "  $0 *.mp3                        # すべてのMP3ファイルを変換"
    echo
    echo "Azure Speech Services用最適設定:"
    echo "  • サンプリングレート: 16kHz"
    echo "  • チャンネル: モノラル"
    echo "  • フォーマット: WAV (PCM)"
    echo
    exit 1
fi

# Process arguments
success_count=0
error_count=0

for input_file in "$@"; do
    # Skip if not an MP3 file
    if [[ ! "$input_file" =~ \.(mp3|MP3)$ ]]; then
        echo "⚠️  スキップ: $(basename "$input_file") (MP3ファイルではありません)"
        continue
    fi

    # Check if input file exists
    if [ ! -f "$input_file" ]; then
        echo "❌ エラー: ファイルが見つかりません: $input_file"
        ((error_count++))
        continue
    fi

    # Determine output filename
    if [ $# -eq 1 ]; then
        # Single file: replace extension
        output_file="${input_file%.*}.wav"
    elif [ $# -eq 2 ] && [ ! -f "$2" ]; then
        # Two arguments and second is not a file: use as output name
        output_file="$2"
    else
        # Multiple files: replace extension
        output_file="${input_file%.*}.wav"
    fi

    # Check if output file already exists
    if [ -f "$output_file" ]; then
        echo "⚠️  $(basename "$output_file") は既に存在します"
        read -p "上書きしますか？ (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "スキップしました: $(basename "$output_file")"
            echo
            continue
        fi
    fi

    # Convert the file
    if convert_file "$input_file" "$output_file"; then
        ((success_count++))
    else
        ((error_count++))
    fi
done

# Summary
echo "==============================================="
echo "変換完了サマリー:"
echo "✅ 成功: ${success_count}ファイル"
if [ $error_count -gt 0 ]; then
    echo "❌ 失敗: ${error_count}ファイル"
fi
echo

if [ $success_count -gt 0 ]; then
    echo "🎯 次のステップ:"
    echo "変換されたWAVファイルを使用してAzure Speech Servicesで話者分離を実行:"
    echo
    echo "cd singlefile"
    echo "./AzureSpeechDiarization.cs audio.wav"
    echo
    echo "または:"
    echo "dotnet run --file AzureSpeechDiarization.cs -- audio.wav"
fi