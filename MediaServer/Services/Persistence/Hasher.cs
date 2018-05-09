using System;
using System.Security.Cryptography;

namespace MediaServer.Services.Persistence {
    public static class Hasher {
		public static string GetHashOfImage(byte[] imageData) {
            var sha512 = new SHA512Managed();
            var hash = sha512.ComputeHash(imageData);
            var stringRepresentation = BitConverter.ToString(hash).Replace("-", "");
            return stringRepresentation;
        } 
    }
}
