using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Subnet_Calculator.Pages
{
    public class IndexModel : PageModel
    {
        string stringIpAdress;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        [BindProperty] //die Variable ipadress muss ein BindProperty sein, da sie im HTML Teil deklariert wird
        //die Ip-Adresse, welchhe vom Benutzer im HTML Teil eingegeben wird
        public string ipAdress{ get; set; }

        [BindProperty]//die Variable mask muss ein BindProperty sein, da sie im HTML Teil deklariert wird
        public string mask { get; set; }
        public string output { get; set; }
        public string subnetMaskOutput { get;set; }

        public string netID { get; set; }

        public string hostAmount { get; set; }
        public string firstHost { get; set; }
        public string lastHost { get; set; }
        public string broadcast { get; set; }

        //die Variable buttonId muss ein BindProperty sein, da sie im HTML Teil deklariert wird
        [BindProperty] public string buttonId { get; set; }

        //statische Liste, welche alle Subnetze speichert
        //diese Liste wird im HTML Teil in einer Tabelle ausgegeben
        public static List<Subnet> allSubnets = new List<Subnet>();

        //getter, welche die statisch gesetzte Liste zurückgibt
        public List<Subnet> getList()
        {
            return allSubnets;
        }

        //Methode, welche die statische Liste leert
        public void clearSubnetList()
        {
            allSubnets.Clear();
        }

        //Funktion, welche zu der statischen Klasse allSubnets einen neuen Eintrag hinzufügt
        public void addToList(Subnet net)
        {
            allSubnets.Add(net);
        }

        //diese Funktion wird ausgeführt, wenn jemand auf den Berechnen Knopf im HTML Teil drückt
        public void OnPost()
        {
            //Erstellt eine Instanz der Klasse Subnet
            Subnet subnet = new Subnet();

            string stringIPAdress = ipAdress;
            string stringMask = mask;

            //wenn die IpAdresse und die Maske eingebeben wurden
            if (ipAdress == null || mask == null)
            {
                //gibt eine Fehlermeldung aus
                output = "Bitte gib eine IP-Adresse und die Maske ein!";
            }
            else
            {
                subnet.ipAdress = ipAdress;
                subnet.mask = mask;

                //Quelle des Regex: https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
                string strRegex = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)(\\.(?!$)|$)){4}$";
                //Quellen Ende

                //der Regex wird mit dem Regex String erstellt
                Regex re = new Regex(strRegex);

                //speichert als boolean ab, ob die Maske eine Zahl ist
                bool isNumeric = int.TryParse(mask, out _);

                //wenn das Regex muster passt und die Maske eine Zahl zwischen 0 und 32 ist, dannn ist es ein gültiger Input und damit kann weiter gearbeitet werden
                if ((re.IsMatch(stringIPAdress)) && isNumeric && Convert.ToInt32(mask) > 0 && Convert.ToInt32(mask) <= 32)
                { 
                    //checke ob die eingegebene Maske auch eine Zahl ist
                    isNumeric = int.TryParse(stringMask, out _);
                    //wenn es eine Zahl ist, also ein valider Input
                    if (isNumeric)
                    {
                        //der Berechnen Button wurde neu gedrückt. Die alte Liste wird entfernt und neu beschrieben
                        clearSubnetList();

                        //das Subnet verfügt über eine Ip-Adresse und eine Maske, es berechnet sich nun selbst
                        subnet.calculateWholeSubnet();

                        //das generierte Subnet in der Subnet Liste eingetragen, diese Liste wird im HTML ausgegeben
                        allSubnets.Add(subnet);
                    }
                    else //wenn die Maske keine Zahl ist wird ein Fehler ausgegeben
                    {

                        output = "die Maske muss eine Zahl sein!";
                    }
                }
                else //wenn der Input NICHT dem Regex Muster entspricht oder die Zahl kleiner als 0 oder größer als 32 ist, ist es eine Fehleingabe
                {
                    //gibt die Fehlermeldung aus
                    output = "Gib eine gültige IPv4 Adresse und Maske ein!";
                }
            }
        }

        //diese Methode teilt ein vorhandenes Subnetz in zwei kleiner Subnetzte
        public void OnPostDivideSubnet()
        {
            int clickedButtonId = Convert.ToInt32(buttonId);
            int currentMask = Convert.ToInt32(getList()[clickedButtonId].mask);

            //Abfrage, ob die Masker kleiner oder gleich 30 ist
            if (Convert.ToInt32(getList()[clickedButtonId].mask) <= 30)
            {
                //auf die bisherige Subnetzmaske wird 1 addiert. Das Subnet wird kleiner, wenn die Subnetzmakse kleiner wird.
                int maskPlus1 = (currentMask + 1);

                //das 1. neue Subnet wird berechnet
                Subnet subnet1 = new Subnet();

                //die Ip-Adresse des 1. neuen Subnetzes ist die netAdresse des zu teilenden Subnetzes
                subnet1.ipAdress = getList()[clickedButtonId].netAdressDecimal;

                //die Subnet bekommt die neue Maske: alte Maske+1
                subnet1.mask = maskPlus1.ToString();

                //das neue Subnetz hat eine Ip-Adresse und Maske und berechnet sich nun selbst
                subnet1.calculateWholeSubnet();

                //das 2. neue Subnet wird berechnet
                Subnet subnet2 = new Subnet();
                string temp = subnet2.calculateFirstHostOfSecondSubnet((subnet1.broadcast));
                subnet2.ipAdress = temp;

                //das Subnet bekommt die neue Maske: alte Maske+1
                subnet2.mask = maskPlus1.ToString();

                //das neue Subnetz hat eine Ip-Adresse und Maske und berechnet sich nun selbst
                subnet2.calculateWholeSubnet();

                //das 1. neue Subnet wird hinter dem Subnet eingefügt, welches geteilt wurde
                allSubnets.Insert(clickedButtonId + 1, subnet1);

                //das 2. neue Subnet wird hinter das 1. neue Subnet eingefügt
                allSubnets.Insert(clickedButtonId + 2, subnet2);

                //das geteilt Subnet(mit dem Index des Buttons) wird entfernt,
                // zurück bleiben die beiden neu generierten Subnetze, und das alte Subnet ist geteilt.
                allSubnets.RemoveAt(clickedButtonId);
            }
            else
            {
               //Abfrage, ob die Subnetzmaske 32 ist. Dabei handelt es sich um einen Spezialfall, der separat berechnet wird
                if (currentMask == 32)
                {
                    allSubnets[clickedButtonId].exceptionForCIDR32(); 
                }

                //Abfrage, ob die Subnetzmaske 31 ist. Dabei handelt es sich um einen Spezialfall, der separat berechnet wird
                if (currentMask == 31)
                {
                    //noch das Subnetz teilen
                    Subnet subnet1 = new Subnet();
                    subnet1.mask = "32";
                    subnet1.ipAdress = getList()[clickedButtonId].ipAdress;
                    subnet1.calculateWholeSubnet();

                    Subnet subnet2 = new Subnet();
                    subnet2.mask = "32";
                    subnet2.ipAdress = getList()[clickedButtonId].lastHost;
                    subnet2.calculateWholeSubnet();

                    //das Subnetz wird in der Liste hinter dem geteilten Subnetz eingefügt
                    allSubnets.Insert(clickedButtonId + 1, subnet1);

                    //Das 2. neue Subnetz wird hinter das 1. neue Subnetz eingefügt 
                    allSubnets.Insert(clickedButtonId + 2, subnet2);

                    //das Subnet, welches geteilt wurde wird entfernt. Die beiden neuen Subnetzt wurden eingetragen, daher wurde das Subnet erfolgreich geteilt.
                    allSubnets.RemoveAt(clickedButtonId);

                }
            }
        }
    }
}