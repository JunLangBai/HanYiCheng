using System;
using System.IO;
using UnityEngine;

public static class SavWav
{
    private const int HEADER_SIZE = 44;

    /// <summary>
    /// (旧方法) 接收 AudioClip 对象并保存。为了兼容性保留，但当前方案不直接使用。
    /// </summary>
    public static bool Save(string filepath, AudioClip clip)
    {
        if (!filepath.EndsWith(".wav"))
        {
            filepath += ".wav";
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = new FileStream(filepath, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            // --- 以下是修正部分 ---
            writer.Write(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
            writer.Write(0);
            writer.Write(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((ushort)(clip.channels * 2));
            writer.Write((ushort)16);
            writer.Write(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
            writer.Write(0);
            // --- 修正结束 ---

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            for (int i = 0; i < samples.Length; i++)
            {
                short intSample = (short)(samples[i] * short.MaxValue);
                writer.Write(intSample);
            }

            long fileSize = writer.BaseStream.Length;
            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((int)(fileSize - 8));
            writer.Seek(40, SeekOrigin.Begin);
            writer.Write((int)(fileSize - HEADER_SIZE));
        }

        return true;
    }
    
    /// <summary>
    /// (新方法) 接收原始 float 数组并保存为 WAV 文件，专为后台线程设计。
    /// </summary>
    public static bool Save(string filepath, float[] samples, int frequency, int channels)
    {
        if (!filepath.EndsWith(".wav"))
        {
            filepath += ".wav";
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = new FileStream(filepath, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            // --- 以下是修正部分 ---
            writer.Write(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
            writer.Write(0); 
            writer.Write(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2);
            writer.Write((ushort)(channels * 2));
            writer.Write((ushort)16);
            writer.Write(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
            writer.Write(0);
            // --- 修正结束 ---

            for (int i = 0; i < samples.Length; i++)
            {
                short intSample = (short)(samples[i] * short.MaxValue);
                writer.Write(intSample);
            }

            long fileSize = writer.BaseStream.Length;
            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((int)(fileSize - 8));
            writer.Seek(40, SeekOrigin.Begin);
            writer.Write((int)(fileSize - HEADER_SIZE));
        }

        return true;
    }
}