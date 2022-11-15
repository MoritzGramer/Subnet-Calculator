using System.Net;

namespace Subnet_Calculator
{
    public class Subnet
    {

        //gegeben als Input in der Index Datei
        public string mask = "";

        //gegeben als Input in der Index Datei
        public string ipAdress = "";

        //die eingegebene IP-Adresse in binärer Schreibweise
        public string binaryIpAdress;

        public string netAdress;
        public string netAdressDecimal;

        public string broadcast;

        public string netId;
        public string subnetzmask;
        public string firstHost;
        public string hostAmount;
        public string lastHost;
        public string subnetMaskOutput;


        //Konstruktor der Subnet Klasse mit Parametern
        public Subnet(string ip, string mask)
        {
            this.ipAdress = ip;
            this.mask = mask;  
        }

        //Konstruktor der Subnet Klasse ohne Parameter
        public Subnet(){}

        //die Klasse ruft selbständig die eigenen Methoden auf und berechnet somit das eigene Subnet
        public void calculateWholeSubnet()
        {
            //wenn die beiden Eingaben vorhanden sind
            if(this.mask != "" && this.ipAdress != null)
            {

                //wenn die eingegeben Maske 30 oder kleiner wird das gesamte Subnet berechnet. 
                //Die Spezialfälle der CIDR 31 und 32 werden separat berechnet.
                if (Convert.ToInt32(this.mask) <= 30)
                {
                    //die vom Benutzer eingegebene Ip-Adresse wird in Binärform umgewandelt
                    this.binaryIpAdress = this.getBinaryIpAdress(this.ipAdress);

                    //die Subnetzmakse in Binärform wird berechnet
                    this.calculateSubnetMask();

                    //die netAdresse in Binärform wird berechnet
                    this.netAdress = this.getNetAdress(this.binaryIpAdress, this.subnetzmask);

                    //die netAdresse wird berechnet und returned
                    this.netAdressDecimal = this.binaryIpAdressToDecimal(this.netAdress);

                    //der Outpur für die Subnetzmaske wird berechnet.
                    this.subnetMaskOutput = this.binaryIpAdressToDecimal(this.subnetzmask);

                    //der erste Host wird berechnet und returned
                    this.firstHost = this.binaryIpAdressToDecimal(getFirstHost(this.netAdress));

                    //die Anzahl der möglichen Hosts werden berechnet und returned
                    this.hostAmount = this.getAmountOfHosts(this.mask).ToString();

                    //der broadcast wird berechnet und returned
                    this.broadcast = this.binaryIpAdressToDecimal(calculateBroadcast(this.netAdress));

                    //der letzte Host wird berechnet und returned
                    this.lastHost = this.binaryIpAdressToDecimal(getLastHost(this.netAdress));

                }
                //wenn die eingegeben Maske größer ist als 30 sind es Spezialfälle und werden hier separat bearbeitet.
                else {

                    //für cidr 32 gibt es eigene "Gesetze", deswegen wird eine Extra Methode aufgerufen, welche dieses Subnet anders als sont berechnet
                    if (Convert.ToInt32(this.mask) == 31)
                    {
                       
                        exceptionForCIDR31();
                    }

                    //für cidr 32 gibt es eigene "Gesetze", deswegen wird eine Extra Methode aufgerufen, welche dieses Subnet anders als sont berechnet
                    if(Convert.ToInt32(this.mask) == 32)
                    {
                        exceptionForCIDR32();
                    }


                }
            }
        }

        //berechnet die NetAdresse
        public string getNetAdress(string ipAdressInBinary, string subnetmask)
        {
            string netAdress = "";

            //iteriert durch jedes Bit der Adresse
            for (int i = 0; i < 32; i++)
            {
                //wenn bei Beiden Adressen eine 1 steht wird auch die 1 weiter geschrieben (logisches UND)
                if (ipAdressInBinary.ElementAt(i).Equals('1') && subnetmask.ElementAt(i).Equals('1'))
                {
                    netAdress += "1";
                }
                else
                {
                    netAdress += "0";
                }
            }
            //gibt die generierte NetAdresse zurück
            return netAdress;
        }

        //schreibt die gegebene IP Adresse in die Binäre Schreibweise um, damit sie Verundet werden kann
        public string getBinaryIpAdress(string IpAdress)
        {
            //dieser String wird die IP-Adresse in Binär sein und später returned
            string binaryIp = "";
            string IpAdressPlusDot = IpAdress + ".";

            //eine Zwischenspeicher Variable. Der Inhalt wird dann zu einem Int umgewandelt
            string temp = "";

            string binary;
            int tempInt;

            //iteriert durch die IpAdresse Stück für Stück
            for (int i = 0; i < IpAdressPlusDot.Length; i++)
            {
                //An der Stelle, an dem das aktuelle Element ein '.' ist wird der Tempstring geleert und in int umgewandelt
                if (IpAdressPlusDot.ElementAt(i).Equals('.'))
                { 

                    tempInt = Convert.ToInt32(temp);
                    binary = Convert.ToString(tempInt, 2);

                    while (binary.Length < 8)
                    {
                        binary = "0" + binary;
                    }
                    binaryIp += binary;
                    temp = "";

                }
                else
                {
                    //temp wird erweitert bis man auf einen '.' trifft
                    temp += IpAdressPlusDot.ElementAt(i);
                }
            }
            return binaryIp;
        }

        //ändert die Ip Adresse von Binärer Schreibeweise in die Decimale Schreibweise
        public string binaryIpAdressToDecimal(string netId)
        {

            //nimmt sich die ersten 8 Zeichen(das erste Byte) und wandelt es in Dezimal um
            string firstByte = Convert.ToInt32(netId.Substring(0, 8), 2).ToString();

            //nimmt sich den 2. Block von  8 Zeichen(das zweite Byte) und wandelt es in Dezimal um
            string secondByte = Convert.ToInt32(netId.Substring(8, 8), 2).ToString();

            //nimmt sich den 3. Block von  8 Zeichen(das dritte Byte) und wandelt es in Dezimal um
            string thirdByte = Convert.ToInt32(netId.Substring(16, 8), 2).ToString();

            //nimmt sich den 4. Block von  8 Zeichen(das vierte Byte) und wandelt es in Dezimal um
            string fourthByte = Convert.ToInt32(netId.Substring(24, netId.Length-24), 2).ToString();

            return firstByte + "." + secondByte + "." + thirdByte + "." + fourthByte;
        }

        public string getFirstHost(string ipAdressInBinary)
        {
            string firstHost = "";
            firstHost = ipAdressInBinary;

            firstHost = firstHost.Remove(firstHost.Length - 1, 1);
            firstHost += "1";

            string lastByte = ipAdressInBinary.Substring(23, 8);

            //wenn das letzte Byte eine ungerade Zahl ist
            if (!(Convert.ToInt32(lastByte, 2) % 2 == 0))
            {
                int lastBytePlus1 = Convert.ToInt32(lastByte, 2);
                lastBytePlus1 += 1;
                firstHost = ipAdressInBinary.Substring(0, 24) + Convert.ToString(lastBytePlus1, 2);
            }
            return firstHost;
        }

        //berechnet die möglichen Hosts im Subnetz.. Formel 2 hoch n -2
        public int getAmountOfHosts(string mask)
        {
            //die hosts werden mit 0 initialisiert
            double hosts = 0;

            //berechnet die Anzahl an Hostbits 
            int amountOfHostbits = 32 - Convert.ToInt32(mask);

            //2 hoch n wird berechnet 
            hosts = Math.Pow(2, amountOfHostbits);

            return (int)hosts - 2;
        }

        //die Broadcast IP wird berechnet.. das letzte Hostbit wird auf '1' gesetzt, alle anderen auf 0.. davor steht die NetID
        public string calculateBroadcast(string netIdInBinary)
        {
            string broadcast = "";
            string temp = "";

            //iteriert durch die NetAdresse in Binäeschreibweise bis auf den letzt Byte. Dieser kann so später einfacher hinzugefügt werden
            for (int i = 0; i < netIdInBinary.Length - 1; i++)
            {
                if (i < Convert.ToInt32(mask))
                {
                    //die netAdresse wird aufgeschrieben bis die Hostbits kommen
                    temp += netIdInBinary.ElementAt(i);
                }
                else
                {
                    //für die Hostbits werden '0' aufgeschrieben. bis auf das letzte Bit später
                    temp += "1";
                }
            }
            //das letzte Bit wird auf '1' gesetzt
            broadcast = temp + "1";

            //der Broadcast wird in Binärschreibweise returned
            return broadcast;
        }

        //der letzte Host wird berechnet.. er berechnet sich: NetId + (2 hoch n)-2
        public string getLastHost(string netIdInBinary)
        {
            string lastHost = "";

            //eine string variable zum Zwischenspeichern der IP bevor der letzt negative Bit hinzugefügt wird
            string temp = "";

            for (int i = 0; i < netIdInBinary.Length - 1; i++)
            {
                if (i < Convert.ToInt32(mask))
                {
                    //die netAdresse wird aufgeschrieben bis die Hostbits kommen
                    temp += netIdInBinary.ElementAt(i);
                }
                else
                {
                    temp += "1";
                }
            }
            lastHost = temp + "0";

            return lastHost;
        }

        public void calculateSubnetMask()
        {
            //stringMask wurde als int identifieziert und kann jetzt umgewandelt werden ohne Gefahr eines Parse Fehlers
            int maskeLentgh = int.Parse(mask);
            string temp = "";

            // Die subnetzmaske wird auf die 32 Bit Länge aufgefüllt
            for (int i = 0; i < 32; i++)
            {
                //Abfrage, wie viele 1en in die Subnetzmaske reingeschrieben werden soll
                if (i >= maskeLentgh)
                {
                    //in die Subnetzmakse wird 0 hinzugefügt
                    temp += "0";
                }
                else
                {
                    //in die Subnetzmakse wird 1 hinzugefügt
                    temp += "1";
                }
            }
            this.subnetzmask = temp;
        }

        //diese Methode berechnet den ersten Host des geteilten Subnets:
        //Dabei ist der 
        public string calculateFirstHostOfSecondSubnet(string ipAdressDecimal)
        {

            //die Adresse, welche als Parameter überliefert wurde wird in Binäre Schreibform umgewandelt
            string ipAdressBinary = getBinaryIpAdress(ipAdressDecimal);

            //das erste Byte der Ip-Adresse
            string firstByte = Convert.ToInt32(ipAdressBinary.Substring(0, 8), 2).ToString();

            //das zweite Byte der Ip-Adresse
            string secondByte = Convert.ToInt32(ipAdressBinary.Substring(8, 8), 2).ToString();

            //das dritte Byte der Ip-Adresse
            string thirdByte = Convert.ToInt32(ipAdressBinary.Substring(16, 8), 2).ToString();

            //das vierte Byte der Ip-Adresse
            string fourthByte = Convert.ToInt32(ipAdressBinary.Substring(24, 8), 2).ToString();
            
            //erstes Byte in Dezimaler Schreibweise
            int firstByteInDecimal = Convert.ToInt32(firstByte);

            //zweites Byte in dezimaler Schreibwesie
            int secondByteInDecimal = Convert.ToInt32(secondByte);

            //drittes Byte in dezimaler Schreibweise
            int thirdByteInDecimal = Convert.ToInt32(thirdByte);

            //viertes Byte in dezimaler Schreibweise
            int lastByteInDecimal = Convert.ToInt32(fourthByte);
            
            //wenn das erste Byte kleiner als 255 ist 
            if(lastByteInDecimal < 255)
            {
                lastByteInDecimal++;
            }
            else
            {
                if(thirdByteInDecimal < 255)
                {
                    thirdByteInDecimal++;
                }
                else
                {
                    if(secondByteInDecimal < 255)
                    {
                        secondByteInDecimal++;
                    }
                    else
                    {
                        firstByteInDecimal++;
                    }
                }
            }

            return (firstByteInDecimal+"."+ secondByteInDecimal + "."+ thirdByteInDecimal + "."+ lastByteInDecimal);
        }

        public void exceptionForCIDR32()
        {
            this.hostAmount = 1.ToString();
            this.netAdressDecimal = this.ipAdress;
            this.firstHost = netAdressDecimal;
            this.subnetMaskOutput = "255.255.255.255";
            this.lastHost = "";
            this.broadcast = "";
        }

        public void exceptionForCIDR31()
        {
            this.hostAmount = 2.ToString();
            this.subnetMaskOutput = "255.255.255.254";
            this.netAdressDecimal = this.ipAdress;
            this.firstHost = this.ipAdress;

            this.broadcast = calculateFirstHostOfSecondSubnet(ipAdress);
            this.lastHost = broadcast;
        }
    }
}
