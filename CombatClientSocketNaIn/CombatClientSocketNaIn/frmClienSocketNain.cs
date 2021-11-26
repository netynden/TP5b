using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CombatClientSocketNaIn.Classes;

//Nicolas Larouche

namespace CombatClientSocketNaIn
{
    public partial class frmClienSocketNain : Form
    {
        Random m_r;
        Elfe m_elfe;
        Nain m_nain;
        Socket m_client;

        public frmClienSocketNain()
        {
            InitializeComponent();
            m_r = new Random();
            btnReset.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            Reset();
        }
        void Reset()
        {
            m_nain = new Nain(m_r.Next(5, 10), m_r.Next(2, 6), m_r.Next(0, 3));
            picNain.Image = m_nain.Avatar;
            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString(); ;
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme;

            m_elfe = new Elfe(1, 0, 0);
            picElfe.Image = m_elfe.Avatar;
            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnFrappe.Enabled = true;
            Reset();
        }

        private void btnFrappe_Click(object sender, EventArgs e)
        {
            // déclarations de variables locales 
            string reponse = "aucune",str =" ";
            int nbOctetReception;
            int vieNain = 0, forceNain = 0, vieElfe = 0, forceElfe = 0, sortElfe = 0;
            string armeNain;
            string[] strTab;
            byte[] tByteReception = new byte[50];
            byte[] tByteEnvoie;
            ASCIIEncoding textByte = new ASCIIEncoding();

            //Disable le bouton
            btnFrappe.Enabled = false;
            btnReset.Enabled = false;

            try
            {
                //Se connecter au serveur
                m_client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                m_client.Connect(IPAddress.Parse("127.0.0.1"), 8888);

                MessageBox.Show("Assurez vous que le serveur est démarré et en attente");

                //Code si connection éffectué
                if (m_client.Connected)
                {
                    //Construction de la string
                    str = m_nain.Vie.ToString() + ";" + m_nain.Force + ";" + m_nain.Arme + ";";

                    //Affichage de la transmission
                    MessageBox.Show("Client: \r\nTransmet..." + str);

                    //Envoie de la string
                    tByteEnvoie = textByte.GetBytes(str);
                    m_client.Send(tByteEnvoie);
                    Thread.Sleep(500);

                    //Réception
                    nbOctetReception = m_client.Receive(tByteReception);

                    //Convertir le tByte en string
                    reponse = Encoding.ASCII.GetString(tByteReception);
                    MessageBox.Show("Serveur renvoie: " + reponse);

                    //Split du string
                    strTab = reponse.Split(';');
                    vieNain = Convert.ToInt32(strTab[0]);
                    forceNain = Convert.ToInt32(strTab[1]);
                    armeNain = strTab[2];
                    vieElfe = Convert.ToInt32(strTab[3]);
                    forceElfe = Convert.ToInt32(strTab[4]);
                    sortElfe = Convert.ToInt32(strTab[5]);


                    //Affectation des paramètres
                    m_nain.Vie = vieNain;
                    m_nain.Force = forceNain;
                    m_nain.Arme = armeNain;
                    m_elfe.Vie = vieElfe;
                    m_elfe.Force = forceElfe;
                    m_elfe.Sort = sortElfe;

                    //Afficher les stats
                    lblVieNain.Text = "Vie: " + m_nain.Vie.ToString();
                    lblForceNain.Text = "Force: " + m_nain.Force.ToString();
                    lblArmeNain.Text = "Arme: " + m_nain.Arme;

                    lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
                    lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
                    lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();

                    //Tester et afficher le gagnant
                    if (m_nain.Vie <= 0 && m_elfe.Vie <= 0)
                    {
                        picElfe.Image = Image.FromFile("ninja.jpg");
                        picNain.Image = Image.FromFile("ninja.jpg");
                        MessageBox.Show("Les deux sont mort !");
                    }

                    else if (m_nain.Vie <= 0)
                    {
                        picNain.Image = m_elfe.Avatar;
                        MessageBox.Show("Nain est mort");
                    }
                    else if(m_elfe.Vie <= 0)
                    {
                        picElfe.Image = m_nain.Avatar;
                        MessageBox.Show("Elfe est mort");
                    }
                }
                //Fermer la connection
                m_client.Close();
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //Enable les boutons
            btnFrappe.Enabled = true;
            btnReset.Enabled = true;


        }
    }
}
