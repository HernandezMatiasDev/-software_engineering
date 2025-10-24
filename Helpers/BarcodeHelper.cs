// En SuMejorPeso/Helpers/BarcodeHelper.cs
using System;
using System.Linq;

namespace SuMejorPeso.Helpers
{
    public static class BarcodeHelper
    {
        /// <summary>
        /// Genera un código de 13 dígitos tipo EAN-13 a partir de un dato base (ej: un DNI).
        /// </summary>
        /// <param name="baseData">El dato base, como un DNI o un ID.</param>
        /// <returns>Un string de 13 dígitos con un dígito de control válido.</returns>
        public static string GenerateEAN13(string baseData)
        {
            // 1. Limpiamos el DNI para que sean solo números
            string numericData = new string(baseData.Where(char.IsDigit).ToArray());

            // 2. Lo ajustamos a 12 dígitos (el EAN-13 usa 12 + 1 de control)
            if (numericData.Length > 12)
            {
                // Si es muy largo (ej: un pasaporte), tomamos los últimos 12
                numericData = numericData.Substring(numericData.Length - 12);
            }
            else
            {
                // Si es muy corto, rellenamos con ceros a la izquierda
                numericData = numericData.PadLeft(12, '0');
            }

            // 3. Calculamos el dígito de control
            string checkDigit = CalculateEAN13CheckDigit(numericData);
            
            // 4. Devolvemos el código completo
            return numericData + checkDigit;
        }

        /// <summary>
        /// Valida si un código de barras de 13 dígitos tiene un dígito de control correcto.
        /// </summary>
        public static bool ValidateEAN13(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length != 13 || !barcode.All(char.IsDigit))
            {
                return false;
            }

            try
            {
                string first12 = barcode.Substring(0, 12);
                string lastDigit = barcode.Substring(12, 1);
                
                // Recalcula el dígito de control y comprueba si coincide
                return (CalculateEAN13CheckDigit(first12) == lastDigit);
            }
            catch
            {
                return false; // Error en el parseo
            }
        }

        /// <summary>
        /// Calcula el dígito de control EAN-13 (Algoritmo Módulo 10).
        /// </summary>
        private static string CalculateEAN13CheckDigit(string twelveDigits)
        {
            if (twelveDigits.Length != 12)
            {
                throw new ArgumentException("La base debe tener 12 dígitos.");
            }

            int sumOdd = 0;  // Suma de posiciones impares (1, 3, 5...)
            int sumEven = 0; // Suma de posiciones pares (2, 4, 6...)

            for (int i = 0; i < 12; i++)
            {
                int digit = int.Parse(twelveDigits[i].ToString());

                // (i+1) porque las posiciones son 1-indexadas, no 0-indexadas
                if ((i + 1) % 2 == 0)
                {
                    // Posición PAR: Multiplica por 3
                    sumEven += digit;
                }
                else
                {
                    // Posición IMPAR: Multiplica por 1
                    sumOdd += digit;
                }
            }

            // 1. Suma total ponderada
            int totalSum = sumOdd + (sumEven * 3);

            // 2. Encuentra el residuo al dividir por 10
            int remainder = totalSum % 10;

            // 3. El dígito de control es lo que falta para llegar al siguiente múltiplo de 10
            int checkDigit = (remainder == 0) ? 0 : (10 - remainder);
            
            return checkDigit.ToString();
        }
    }
}