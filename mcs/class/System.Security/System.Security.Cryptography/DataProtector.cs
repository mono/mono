using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Crypto = System.Security.Cryptography.Translation;

namespace System.Security.Cryptography
{
  public abstract class DataProtector
  {
    private string m_applicationName;
    private string m_primaryPurpose;
    private IEnumerable<string> m_specificPurposes;
    private volatile byte[] m_hashedPurpose;

    protected DataProtector(string applicationName, string primaryPurpose, string[] specificPurposes)
    {
      if (string.IsNullOrWhiteSpace(applicationName))
        throw new ArgumentException(Crypto.SR.Cryptography_DataProtector_InvalidAppNameOrPurpose, nameof (applicationName));
      if (string.IsNullOrWhiteSpace(primaryPurpose))
        throw new ArgumentException(Crypto.SR.Cryptography_DataProtector_InvalidAppNameOrPurpose, nameof (primaryPurpose));
      if (specificPurposes != null)
      {
        foreach (string specificPurpose in specificPurposes)
        {
          if (string.IsNullOrWhiteSpace(specificPurpose))
            throw new ArgumentException(Crypto.SR.Cryptography_DataProtector_InvalidAppNameOrPurpose, nameof (specificPurposes));
        }
      }
      this.m_applicationName = applicationName;
      this.m_primaryPurpose = primaryPurpose;
      List<string> stringList = new List<string>();
      if (specificPurposes != null)
        stringList.AddRange((IEnumerable<string>) specificPurposes);
      this.m_specificPurposes = (IEnumerable<string>) stringList;
    }

    protected string ApplicationName
    {
      get
      {
        return this.m_applicationName;
      }
    }

    protected virtual bool PrependHashedPurposeToPlaintext
    {
      get
      {
        return true;
      }
    }

    protected virtual byte[] GetHashedPurpose()
    {
      if (this.m_hashedPurpose == null)
      {
        using (HashAlgorithm hashAlgorithm = HashAlgorithm.Create("System.Security.Cryptography.Sha256Cng"))
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) new CryptoStream((Stream) new MemoryStream(), (ICryptoTransform) hashAlgorithm, CryptoStreamMode.Write), (Encoding) new UTF8Encoding(false, true)))
          {
            binaryWriter.Write(this.ApplicationName);
            binaryWriter.Write(this.PrimaryPurpose);
            foreach (string specificPurpose in this.SpecificPurposes)
              binaryWriter.Write(specificPurpose);
          }
          this.m_hashedPurpose = hashAlgorithm.Hash;
        }
      }
      return this.m_hashedPurpose;
    }

    public abstract bool IsReprotectRequired(byte[] encryptedData);

    protected string PrimaryPurpose
    {
      get
      {
        return this.m_primaryPurpose;
      }
    }

    protected IEnumerable<string> SpecificPurposes
    {
      get
      {
        return this.m_specificPurposes;
      }
    }

    public static DataProtector Create(string providerClass, string applicationName, string primaryPurpose, params string[] specificPurposes)
    {
      if (providerClass == null)
        throw new ArgumentNullException(nameof (providerClass));
      return (DataProtector) CryptoConfig.CreateFromName(providerClass, (object) applicationName, (object) primaryPurpose, (object) specificPurposes);
    }

    public byte[] Protect(byte[] userData)
    {
      if (userData == null)
        throw new ArgumentNullException(nameof (userData));
      if (this.PrependHashedPurposeToPlaintext)
      {
        byte[] hashedPurpose = this.GetHashedPurpose();
        byte[] numArray = new byte[userData.Length + hashedPurpose.Length];
        Array.Copy((Array) hashedPurpose, 0, (Array) numArray, 0, hashedPurpose.Length);
        Array.Copy((Array) userData, 0, (Array) numArray, hashedPurpose.Length, userData.Length);
        userData = numArray;
      }
      return this.ProviderProtect(userData);
    }

    protected abstract byte[] ProviderProtect(byte[] userData);

    protected abstract byte[] ProviderUnprotect(byte[] encryptedData);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public byte[] Unprotect(byte[] encryptedData)
    {
      if (encryptedData == null)
        throw new ArgumentNullException(nameof (encryptedData));
      if (!this.PrependHashedPurposeToPlaintext)
        return this.ProviderUnprotect(encryptedData);
      byte[] numArray1 = this.ProviderUnprotect(encryptedData);
      byte[] hashedPurpose = this.GetHashedPurpose();
      bool flag = numArray1.Length >= hashedPurpose.Length;
      for (int index = 0; index < hashedPurpose.Length; ++index)
      {
        if ((int) hashedPurpose[index] != (int) numArray1[index % numArray1.Length])
          flag = false;
      }
      if (!flag)
        throw new CryptographicException(Crypto.SR.Cryptography_DataProtector_InvalidPurpose);
      byte[] numArray2 = new byte[numArray1.Length - hashedPurpose.Length];
      Array.Copy((Array) numArray1, hashedPurpose.Length, (Array) numArray2, 0, numArray2.Length);
      return numArray2;
    }
  }
}
