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
using CombatServeurSocketElfe.Classes;

//Nicolas Larouche

namespace CombatServeurSocketElfe
{
    public partial class frmServeurSocketElfe : Form
    {
        Random m_r;
        Nain m_nain;
        Elfe m_elfe;
        TcpListener m_ServerListener;
        Socket m_client;
        Thread m_thCombat;

        public frmServeurSocketElfe()
        {
            InitializeComponent();
            m_r = new Random();
            btnReset.Enabled = false;
            //Démarre un serveur de socket (TcpListener)
            m_ServerListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            m_ServerListener.Start();
            lstReception.Items.Add("Serveur démarré !");
            lstReception.Items.Add("PRESSER : << attendre un client >>");
            lstReception.Update();
            Control.CheckForIllegalCrossThreadCalls = false;
            Reset();
        }
        void Reset()
        {
            m_nain = new Nain(1, 0, 0);
            picNain.Image = m_nain.Avatar;
            AfficheStatNain();

            m_elfe = new Elfe(m_r.Next(5, 10), m_r.Next(2, 6), m_r.Next(2, 6));
            picElfe.Image = m_elfe.Avatar;
            AfficheStatElfe();
 
            lstReception.Items.Clear();
        }

        void AfficheStatNain()
        {
            lstReception.Items.Add("Nain: " + m_nain.Vie.ToString() + "/" + m_nain.Force + "/" + m_nain.Arme);
            lstReception.Update();

            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString();
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme;

            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        void AfficheStatElfe()
        {
            lstReception.Items.Add("Elfe: " + m_elfe.Vie.ToString() + "/" + m_elfe.Force + "/" + m_elfe.Sort);
            lstReception.Update();

            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();


            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            btnReset.Enabled = false;
            Reset();
        }     

        private void btnAttente_Click(object sender, EventArgs e)
        {
            // Combat par un thread
            ThreadStart codeThread = new ThreadStart(Combat);
            m_thCombat = new Thread(codeThread);
            m_thCombat.Start();
            
        }
        public void Combat() 
        {
            // déclarations de variables locales 
            string reponseServeur = "aucune";
            string receptionClient = "rien";
            int nbOctetReception;
            int vie = 0, force = 0;
            string arme;
            string[] strTab;
            byte[] tByteReception = new byte[50];
            byte[] tByteEnvoie;
            ASCIIEncoding textByte = new ASCIIEncoding();

            try
            {
                // tous le code de traitement
                while(m_nain.Vie > 0 && m_elfe.Vie > 0)
                {
                    m_client = m_ServerListener.AcceptSocket();

                    lstReception.Items.Add("Client branché");
                    lstReception.Update();
                    Thread.Sleep(500);

                    //Réception des clients
                    nbOctetReception = m_client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);

                    //Affichage du text
                    lstReception.Items.Add("Client: " + receptionClient);
                    lstReception.Update();

                    //Split du string
                    strTab = receptionClient.Split(';');
                    vie = Convert.ToInt32(strTab[0]);
                    force = Convert.ToInt32(strTab[1]);
                    arme = strTab[2];

                    //Affectation des paramètres
                    m_nain.Vie = vie;
                    m_nain.Force = force;
                    m_nain.Arme = arme;

                    //Instaurer le nain
                    AfficheStatNain();

                    //Frapper l'elfe
                    m_nain.Frapper(m_elfe);

                    //Afficher stats de l'elfe
                    AfficheStatElfe();

                    //Lancer sort sur le nain
                    m_elfe.LancerSort(m_nain);

                    //Afficher stats du nain
                    AfficheStatNain();
                    AfficheStatElfe();

                    //Envoie des données au client
                    reponseServeur = m_nain.Vie.ToString() + ";" + m_nain.Force + ";" + m_nain.Arme + ";" + m_elfe.Vie + ";" + m_elfe.Force + ";" + m_elfe.Sort + ";";

                    //Affichage de la réponse serveur
                    lstReception.Items.Add(reponseServeur);
                    lstReception.Update();

                    //Mettre la réponse dans un tableau de bytes
                    tByteEnvoie = textByte.GetBytes(reponseServeur);

                    //Envoie de la réponse
                    m_client.Send(tByteEnvoie);
                    Thread.Sleep(500);

                    //Vérifier si il ya un gagnant
                    if (m_nain.Vie <= 0 && m_elfe.Vie <= 0)
                    {
                        MessageBox.Show("Les deux sont mort !");
                    }

                    else if (m_nain.Vie <= 0)
                    {
                        MessageBox.Show("Nain est mort");
                    }
                    else if (m_elfe.Vie <= 0)
                    {
                        MessageBox.Show("Elfe est mort");
                    }

                    //Fermer le socket
                    m_client.Close();
                }
                btnReset.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // il faut avoir un objet elfe et un objet nain instanciés
            m_elfe.Vie = 0;
            m_nain.Vie = 0;
            try
            {
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void frmServeurSocketElfe_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnFermer_Click(sender,e);
            try
            {
                // il faut avoir un objet TCPListener existant
                m_ServerListener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}
