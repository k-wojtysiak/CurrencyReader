using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";


        char[] separators = { ',', '.' };   // Chars used to separate currency from subcurrency

        int subLength = 2;                  // Number of numericals used for denoting subcurrency (2 -> $0.XX)

        // All written text in english:
        string wrongFormatMsg = "Incorrect number format";
        string wrongValueMsg = "Incorrect value";

        string connector = "and";
        string[] currencyBig = { "dollar", "dollars" };
        string[] currencySmall = { "cent", "cents" };

        string[] nums = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
                          "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"};
        string[] numsTens = { "zero", "ten", "twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        string[] numScale = { "hundred", "thousand", "million" };


        static void Main(string[] args)
        {
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            
            listener.Start();
            Console.WriteLine("Listening...");

            while (true) //keeps server in listening mode
            {
                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();

                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received : " + dataReceived);

                //APPLY THE CONVERSION ALGORITHM
                Program p = new Program();
                dataReceived = p.Conversion(dataReceived);

                //---write back the text to the client---
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(dataReceived);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                Console.WriteLine("Sending back : " + dataReceived);
                client.Close();
            }
        }



        //Converts numerical notation of currency into textual representation
        private string Conversion(string input)
        {
            string output = "";
            int valBig = 0;
            int valSmall = 0;

            //Check if number format is correct
            try
            {
                //Chcek if number has separator
                bool hasSeparator = false;
                foreach (char sep in separators)
                {
                    if (input.Contains("" + sep))
                        hasSeparator = true;
                }

                //Convert string(s) to int values
                if (hasSeparator)
                {
                    //Split
                    string[] strList = input.Split(separators, 2);

                    //Convert currency
                    if (strList[0] != null)
                        valBig = int.Parse(strList[0]);

                    //Convert subcurrency
                    if (strList[1] != null)
                    {
                        //if there are too many numbers after separator
                        if (strList[1].Length > subLength)
                            throw new Exception();

                        valSmall = int.Parse(strList[1]);

                        //accomodate for short notation (0.1 -> 10 cents)
                        if (strList[1].Length == 1)
                            valSmall *= 10;
                    }
                }
                else
                {
                    valBig = int.Parse(input);
                }

            }
            catch
            {
                return wrongFormatMsg;
            }

            //Check if number value is correct
            if (valBig > 999999999 || valBig < 0 || valSmall < 0)
                return wrongValueMsg;


            // Construct the output string:
            output += ConvertInt(valBig) + " ";         // first number

            if (valBig == 1)                            // currency name
                output += currencyBig[0] + " ";
            else
                output += currencyBig[1] + " ";

            if (valSmall != 0)
            {
                output += connector + " ";              // connector

                output += ConvertInt(valSmall) + " ";   // second number

                if (valSmall == 1)                      // sub-currency name
                    output += currencySmall[0] + " ";
                else
                    output += currencySmall[1] + " ";
            }

            return output;
        }



        // Converts int numbers into their textual representation
        private string ConvertInt(int input)
        {

            if (input < 20)
                return nums[input];

            string output = "";

            if (input >= 1000000)
            {
                output += ConvertInt(input / 1000000) + " " + numScale[2] + " ";
                input %= 1000000;
            }

            if (input >= 1000)
            {
                output += ConvertInt(input / 1000) + " " + numScale[1] + " ";
                input %= 1000;
            }

            if (input >= 100)
            {
                output += ConvertInt(input / 100) + " " + numScale[0] + " ";
                input %= 100;
            }

            if (input >= 20)
            {
                output += numsTens[input / 10];
                input %= 10;
                if (input > 0)
                    output += "-" + nums[input];
            }
            else if (input > 0)
            {
                output += nums[input];
            }

            //Remove repeating spaces (in specific cases)
            if (output.Contains("  "))
                output = string.Join(" ", output.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return output;
        }
    }
}
