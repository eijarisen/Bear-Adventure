using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace BearAdventure.Domain.World;

public sealed class WorldSeed
{
    public const int GeneratorVersion = 1;

    public WorldSeed(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("World seed cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public ulong Derive(int islandId, string streamName)
    {
        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name cannot be empty.", nameof(streamName));
        }

        byte[] input = Encoding.UTF8.GetBytes(
            $"BearAdventure|v{GeneratorVersion}|{Value}|island:{islandId}|stream:{streamName}");

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(input, hash);
        return BinaryPrimitives.ReadUInt64LittleEndian(hash);
    }
}
