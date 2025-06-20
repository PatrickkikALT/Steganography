using System.Text;

namespace ConsoleApp1;
using System.Drawing;

#pragma warning disable CA1416
class Program {
  static void Main(string[] args) {
    switch (args[0].ToLower()) {
      case "embed":
        EmbedMessage(args[1], args[2], args[3]);
        break;
      case "decode":
        Console.WriteLine(DecodeMessage(args[1]));
        break;
      default:
        Console.WriteLine("Invalid command");
        break;
    }
  }

  public static void EmbedMessage(string imagePath, string outputPath, string message) {
    Bitmap bmp = new Bitmap(imagePath);
    string binary = StringToBinary(message);
    
    int index = 0;
    for (int y = 0; y < bmp.Height; y++) {
      for (int x = 0; x < bmp.Width; x++) {
        Color pixel = bmp.GetPixel(x, y);
        if (index >= binary.Length) break;

        byte r = pixel.R;
        byte g = pixel.G;
        byte b = pixel.B;
        
        //embed binary data into the image to create a message that can later be decoded at the end of every pixel.
        //every time an if statement's requirements get met it does index++ so we need to check again for every color value
        r = EmbedBit(r, binary, ref index);
        g = EmbedBit(g, binary, ref index);
        b = EmbedBit(b, binary, ref index);
        bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
      }
    }
    bmp.Save(outputPath);
  }

  public static string DecodeMessage(string imagePath) {
    Bitmap bmp = new Bitmap(imagePath);
    StringBuilder binary = new StringBuilder();
    StringBuilder text = new StringBuilder();
    for (int y = 0; y < bmp.Height; y++) {
      for (int x = 0; x < bmp.Width; x++) {
        Color pixel = bmp.GetPixel(x, y);
        
        //get last bits from rgb
        binary.Append((pixel.R & 1).ToString());
        binary.Append((pixel.G & 1).ToString());
        binary.Append((pixel.B & 1).ToString());

        //when we have 8 bits, convert to a char and append to text
        while (binary.Length >= 8) {
          string byteString = binary.ToString(0, 8);
          binary.Remove(0, 8);
          char c = (char)Convert.ToByte(byteString, 2);
          if (c == '\0') return text.ToString(); // '/0' means end of message
          text.Append(c);
        }
      }
    }
    return text.ToString();
  }
  
  static byte EmbedBit(byte color, string binary, ref int index) {
    //if there's still binary data left, replace the color's last bit with the next bit from the binary message
    //index++ moves to the next bit for the next color
    //if there's no more data, keep the color the same
    return (index < binary.Length) ? (byte)((color & ~1) | (binary[index++] - '0')) : color;
  }
  

  
  private static string StringToBinary(string data) {
    StringBuilder sb = new StringBuilder();
    foreach (char c in data) {
      sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
    }
    return sb.ToString();
  }

}