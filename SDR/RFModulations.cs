using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDR
{
    public class RFModulations
    {
       

    }

    class QPSKModulator
    {
        private Complex[] constellation;

        public QPSKModulator()
        {
            // Define the QPSK constellation points
            constellation = new Complex[]
            {
            new Complex(1.0, 1.0),   // 00
            new Complex(-1.0, 1.0),  // 01
            new Complex(-1.0, -1.0), // 11
            new Complex(1.0, -1.0)   // 10
            };
        }

        public Complex[] Modulate(int[] dataBits)
        {
            int numBits = dataBits.Length;
            if (numBits % 2 != 0)
            {
                throw new ArgumentException("The number of data bits must be even for QPSK modulation.");
            }

            Complex[] modulatedSignal = new Complex[numBits / 2];
            for (int i = 0; i < numBits; i += 2)
            {
                int symbolIndex = dataBits[i] * 2 + dataBits[i + 1];
                modulatedSignal[i / 2] = constellation[symbolIndex];
            }

            return modulatedSignal;
        }

        public byte[] ComplexToBytes(Complex[] complexData)
        {
            byte[] byteArray = new byte[complexData.Length * 2 * sizeof(float)]; // Each complex number represented as two floats

            for (int i = 0; i < complexData.Length; i++)
            {
                // Convert real and imaginary parts to bytes
                byte[] realBytes = BitConverter.GetBytes((float)complexData[i].Real);
                byte[] imagBytes = BitConverter.GetBytes((float)complexData[i].Imaginary);

                // Copy bytes to the output array
                Array.Copy(realBytes, 0, byteArray, i * 8, 4); // Copy real part (4 bytes)
                Array.Copy(imagBytes, 0, byteArray, i * 8 + 4, 4); // Copy imaginary part (4 bytes)
            }

            return byteArray;
        }

        public byte[] CopyRealToBytes(Complex[] complexData)
        {
            byte[] byteArray = new byte[complexData.Length * sizeof(int)];
            

            for (int i = 0; i < complexData.Length; i++)
            {
                byte[] realBytes = BitConverter.GetBytes((int)complexData[i].Real);
                Array.Copy(realBytes, 0, byteArray, i * sizeof(int), sizeof(int));                
            }

            return byteArray;
        }

        public byte[] CopyImaginaryToBytes(Complex[] complexData)
        {
            byte[] byteArray = new byte[complexData.Length * sizeof(int)];

            for (int i = 0; i < complexData.Length; i++)
            {
                byte[] imagBytes = BitConverter.GetBytes((int)complexData[i].Imaginary);
                Array.Copy(imagBytes, 0, byteArray, i * sizeof(int), sizeof(int));
            }

            return byteArray;
        }

        public static void Main()
        {
            QPSKModulator modulator = new QPSKModulator();

            // Generate random data bits (0s and 1s)
            int numBits = 1000; // Change this to the desired number of bits
            Random rand = new Random();
            int[] dataBits = new int[numBits];
            for (int i = 0; i < numBits; i++)
            {
                dataBits[i] = rand.Next(2); // Generate 0 or 1
            }

            // Modulate the data using QPSK
            Complex[] modulatedSignal = modulator.Modulate(dataBits);
            var bytes = modulator.ComplexToBytes(modulatedSignal);
            // Example: Print the first few modulated symbols for verification
            Console.WriteLine("First 10 modulated symbols:");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Modulated Signal: {modulatedSignal[i]}\n Byte Signal: {bytes[i]}");
            }
        }
    }
}
