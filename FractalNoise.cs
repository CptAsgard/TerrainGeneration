using UnityEngine;
using System.Collections;

namespace ex {
    public class FractalNoise {

        public float OffsetX    {get; private set;}
        public float OffsetY    {get; private set;}
        public float Seed       {get; private set;}

        public void SetNoise( float offsetX, float offsetY, float seed ) {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Seed = seed;
        }

        public float GenerateFractal( float amplitude, float frequency, int octaves, float x, float y, float persistence ) {
            float total = 0f;
            float octAmplitude = amplitude;
            float freq = frequency;

            for( int i = 0; i < octaves; i++ ) {
                total += GeneratePerlin( (x + OffsetX) * freq, (y + OffsetY) * freq ) * octAmplitude;
                octAmplitude *= persistence;
                freq *= 2;
            }

            return total;
        }

        public float GeneratePerlin( float x, float y ) {
            return Mathf.PerlinNoise( x + Seed, y + Seed );
        }
    }
}
