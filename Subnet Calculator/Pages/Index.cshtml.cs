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
        [BindProperty] // this line connects this variable from the html part to cs part
        public string ipAdress{ get; set; }

        [BindProperty]
        public string mask { get; set; }
        public string output { get; set; }
        public string subnetMaskOutput { get;set; }

        public string netID { get; set; }

        public string hostAmount { get; set; }
        public string firstHost { get; set; }
        public string lastHost { get; set; }
        public string broadcast { get; set; }

        [BindProperty] public string buttonId { get; set; }

        //statische Liste, welche alle Subnetze speichert
        public static List<Subnet> allSubnets = new List<Subnet>();

        //getter, welche die statisch gesetzte Liste zurückgibt
        public List<Subnet> getList()
        {
            return allSubnets;
        }

        //Methode, welche die statische Liste leer macht
        public void clearSubnetList()
        {
            allSubnets.Clear();
        }
        public void addToList(Subnet net)
        {
            allSubnets.Add(net);
        }
        public void OnPost()
        {
            Subnet subnet = new Subnet();

            string stringIPAdress = ipAdress;
            string stringMask = mask;

            //wenn die IpAdresse und die Maske eingebeben wurden
            if (ipAdress == null || mask == null)
            {
                output = "Bitte gib eine IP-Adresse und die Maske ein!";
            }
            else
            {
                subnet.ipAdress = ipAdress;
                subnet.mask = mask;

                //Quelle des Regex: https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
                string strRegex = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)(\\.(?!$)|$)){4}$";
                //Quellen Ende 

                Regex re = new Regex(strRegex);

                //speichert als boolean ab, ob die Maske eine Zahl ist
                bool isNumeric = int.TryParse(mask, out _);

                //wenn das Regex muster passt und die Maske eine Zahl zwischen 0 und 32 ist, dannn ist es ein gültiger Input und damit wird weiter gerechnet
                if ((re.IsMatch(stringIPAdress)) && isNumeric && Convert.ToInt32(mask) > 0 && Convert.ToInt32(mask) <= 32)
                { 
                    //checke ob die eingegebene Maske auch eine Zahl ist
                    isNumeric = int.TryParse(stringMask, out _);
                    //wenn es eine Zahl ist
                    if (isNumeric)
                    {
                        clearSubnetList();
                        subnet.calculateWholeSubnet();
                        allSubnets.Add(subnet);
                    }
                    else //wenn die Maske keine Zahl ist wird ein Fehler ausgegeben
                    {
                        output = "die Maske muss eine Zahl sein!";
                    }
                }
                else //wenn der Input NICHT dem Regex Muster entspricht
                {
                    output = "Gib eine gültige IPv4 Adresse und Maske ein!";
                }
            }
            // output = "IP: " + stringIPAdress + " Maske: " + stringMask;
        }
        public void OnPostDivideSubnet()
        {
            int clickedButtonId = Convert.ToInt32(buttonId);
            int currentMask = Convert.ToInt32(getList()[clickedButtonId].mask);

            if (Convert.ToInt32(getList()[clickedButtonId].mask) <= 30)
            {
                int maskPlus1 = (currentMask + 1);
                string issd = allSubnets[clickedButtonId].netAdress;

                Subnet subnet1 = new Subnet();
                //subnet1.netAdress = getList()[Convert.ToInt32(buttonId)].netId;
                subnet1.ipAdress = getList()[clickedButtonId].netAdressDecimal;
                subnet1.mask = maskPlus1.ToString();
                subnet1.calculateWholeSubnet();

                Subnet subnet2 = new Subnet();
                string temp = subnet2.calculateFirstHostOfSecondSubnet((subnet1.broadcast));
                subnet2.ipAdress = temp;
                subnet2.mask = maskPlus1.ToString();
                subnet2.calculateWholeSubnet();

                //allSubnets.Clear();
                output = allSubnets[0].netId;
                allSubnets.Insert(clickedButtonId + 1, subnet1);
                allSubnets.Insert(clickedButtonId + 2, subnet2);

                //output = allSubnets[1].netAdress;

                allSubnets.RemoveAt(clickedButtonId);
            }
            else
            {
                if (currentMask == 32)
                {
                    allSubnets[clickedButtonId].exceptionForCIDR32(); 

                }

                if(currentMask == 31)
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

                    //allSubnets[clickedButtonId].exceptionForCIDR31();

                    allSubnets.Insert(clickedButtonId + 1, subnet1);
                    allSubnets.Insert(clickedButtonId + 2, subnet2);

                    //output = allSubnets[1].netAdress;

                    allSubnets.RemoveAt(clickedButtonId);

                }
            }

        }
    }
}