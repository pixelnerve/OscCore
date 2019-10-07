using System;
using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        const int k_ResizeByteHeadroom = 256;
        
        /// <summary>
        /// Read a blob element.
        /// Checks the element type before reading, and does nothing if the element is not a blob.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="copyTo">
        /// The array to copy blob contents into.
        /// Will be resized if it lacks sufficient capacity
        /// </param>
        /// <param name="copyOffset">The index in the copyTo array to start copying at</param>
        /// <returns>The size of the blob if valid, 0 otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadBlobElement(int index, ref byte[] copyTo, int copyOffset = 0)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            switch (Tags[index])
            {
                case TypeTag.Blob:
                    var offset = Offsets[index];
                    // var size = BitConverter.ToInt32(m_SharedBuffer, offset);
                    int size = *(SharedBufferPtr + offset);
                    var dataStart = offset + 4;    // skip the size int
                    if (copyTo.Length <= size)
                        Array.Resize(ref copyTo, size + k_ResizeByteHeadroom);

                    Buffer.BlockCopy(m_SharedBuffer, dataStart, copyTo, copyOffset, size);

                    fixed (byte* copyPtr = &copyTo[copyOffset])
                    {
                        Buffer.MemoryCopy(SharedBufferPtr + dataStart, copyPtr, size, size);
                    }
                    return size;
                default: 
                    return default;
            }
        }
    }
}