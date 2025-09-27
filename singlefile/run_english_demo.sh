#!/bin/bash

# English Audio Diarization Demo Script
# This script demonstrates English speaker diarization using Azure Speech Services

echo "🎙️ English Audio Diarization Demo"
echo "File: 008-domain-modeling-made-functional-part-1-with-scott-wlaschin_compressed.wav"
echo

# Check if environment variables are set
if [ -z "$SPEECH_KEY" ]; then
    echo "❌ エラー: SPEECH_KEY environment variable is not set"
    echo "Please set your Azure Speech Services credentials:"
    echo "export SPEECH_KEY=your_speech_key_here"
    echo "export SPEECH_REGION=eastus"
    echo
    echo "Then run this script again:"
    echo "./run_english_demo.sh"
    exit 1
fi

if [ -z "$SPEECH_REGION" ]; then
    echo "❌ エラー: SPEECH_REGION environment variable is not set"
    echo "Please set your Azure Speech Services region:"
    echo "export SPEECH_REGION=eastus"
    exit 1
fi

echo "✅ Environment variables are set"
echo "Region: $SPEECH_REGION"
if [ ! -z "$SPEECH_ENDPOINT" ]; then
    echo "Custom Endpoint: $SPEECH_ENDPOINT"
fi
echo

# Check if WAV file exists
WAV_FILE="/Users/tomohisa/dev/GitHub/OpenAI_Whisper_SpeechToTextJa/008-domain-modeling-made-functional-part-1-with-scott-wlaschin_compressed.wav"

if [ ! -f "$WAV_FILE" ]; then
    echo "❌ WAV file not found: $WAV_FILE"
    echo "Converting MP3 to WAV..."
    
    MP3_FILE="/Users/tomohisa/dev/GitHub/OpenAI_Whisper_SpeechToTextJa/008-domain-modeling-made-functional-part-1-with-scott-wlaschin_compressed.mp3"
    
    if [ ! -f "$MP3_FILE" ]; then
        echo "❌ MP3 file not found: $MP3_FILE"
        exit 1
    fi
    
    ffmpeg -i "$MP3_FILE" -ar 16000 -ac 1 -f wav "$WAV_FILE"
    
    if [ $? -ne 0 ]; then
        echo "❌ Failed to convert MP3 to WAV"
        exit 1
    fi
    
    echo "✅ MP3 converted to WAV successfully"
fi

echo "✅ WAV file ready: $(basename "$WAV_FILE")"
echo "File size: $(ls -lh "$WAV_FILE" | awk '{print $5}')"
echo

# Run the application
echo "🚀 Starting Azure Speech Services Diarization..."
echo "This will:"
echo "1. Start the application"
echo "2. Select English (US) - option 2"
echo "3. Choose WAV file processing - option 2"
echo "4. Process the audio file for speaker diarization"
echo
echo "Press any key to continue..."
read -n 1

# Start the .NET 10 single-file application
dotnet run --file AzureSpeechDiarization.cs