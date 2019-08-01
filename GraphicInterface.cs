using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Demo.WindowsForms.CustomMarkers;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Interfaz
{
    public partial class GraphicInterface : Form
    {

        #region ATRIBUTES

        GMarkerArrow Airplane;
        GMarkerArrow Airplane2;
        GMarkerGoogle Target;
        GMapOverlay markerOverlay;
        GMapOverlay markerOverlay2;


        //ThreadStart del;
        Thread line;

        DataTable dt;

        bool boolColonists = false;
        bool boolHabitats = false;

        double LatRec = 0;
        double LngRec= 0;
        float spdRec = 0;
        float altRec= 0;
        int timeRec = 0;
        int anttrack = 0;

        public bool rec = false;
        public bool rep = false;
        public SerialPort serialport;
        public double Lat;
        public double Lng;
        public double TargetLat = 19.326882;  //Siempre configurar antes de iniciar el programa
        public double TargetLng = -99.181937;
        public int alt;
        public float spd;
        public string rawdata;
        public int time= 0;
        public int timeColonist = 0;
        public int timeHabitats = 0;



        #endregion

        public GraphicInterface()
        {
            InitializeComponent();

            try
            {
                serialport = new SerialPort();


                String[] ports = SerialPort.GetPortNames();
                Array.Sort(ports);
                cmbPorts.Items.AddRange(ports);
            }
            catch(IOException error)
            {
                MessageBox.Show("Error: " + error.Message);
            }

        }

        private void GraphicInterface_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0); //sobra si tienes la posición en el diseño
            this.Size = new Size(this.Width, Screen.PrimaryScreen.WorkingArea.Size.Height);

            //Falta configurar lo de los mapas y marcadores

            

            GMaps.Instance.Mode = AccessMode.ServerOnly; //Cambiar a ServerandCache para descargar mapas por ese tiempo
            gMapControl1.CacheLocation = @"C:/Users/itan_/Desktop/Telem/TXCache";
            gMapControl2.CacheLocation = @"C:/Users/itan_/Desktop/Telem/TXCache";

            //* --------------MAPAS--------------

            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;

            dt = new DataTable();
            dt.Columns.Add(new DataColumn("Time [s]", typeof(int)));
            dt.Columns.Add(new DataColumn("Speed [ft/s]", typeof(float)));
            dt.Columns.Add(new DataColumn("Altitude [ft]", typeof(float)));
            dt.Columns.Add(new DataColumn("Latitude [°]", typeof(double)));
            dt.Columns.Add(new DataColumn("Longitude [°]", typeof(double)));

            

            gMapControl1.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.CanDragMap = true;
            gMapControl1.Position = new PointLatLng(19.326882, -99.181937); //CAMBIAR A TX 
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 18;
            gMapControl1.AutoScroll = true;

            gMapControl2.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
            gMapControl2.DragButton = MouseButtons.Left;
            gMapControl2.CanDragMap = true;
            gMapControl2.Position = new PointLatLng(19.326882, -99.181937); //CAMBIAR A TX
            gMapControl2.MinZoom = 0;
            gMapControl2.MaxZoom = 24;
            gMapControl2.Zoom = 18;
            gMapControl2.AutoScroll = true;

            //Configuración de la capa de marcadores
            markerOverlay = new GMapOverlay("DAS");
            markerOverlay2 = new GMapOverlay("DAS RECORDED");

            Target = new GMarkerGoogle(new PointLatLng(TargetLat, TargetLng), GMarkerGoogleType.red);
            Target.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            Target.ToolTipText = string.Format("Target: \n Lat: {0} \n Lng: {1} \n Distance:", TargetLat, TargetLng);
            markerOverlay.Markers.Add(Target);
            markerOverlay2.Markers.Add(Target);


            //*---------------MARCADOR DEL AVIÓN---------------
            Airplane = new GMarkerArrow(new PointLatLng(TargetLat, TargetLng));
            Airplane.ToolTipText = "Airplane";
            Airplane.ToolTip.Fill = Brushes.Black;
            Airplane.ToolTip.Foreground = Brushes.White;
            Airplane.ToolTip.Stroke = Pens.Black;
            Airplane.Bearing = 0; // Rotation angle
            Airplane.Fill = new SolidBrush(Color.FromArgb(155, Color.Red)); // Arrow color
            markerOverlay.Markers.Add(Airplane);
            Airplane2 = new GMarkerArrow(new PointLatLng(TargetLat, TargetLng));
            Airplane2.ToolTipText = "Airplane";
            Airplane2.ToolTip.Fill = Brushes.Black;
            Airplane2.ToolTip.Foreground = Brushes.White;
            Airplane2.ToolTip.Stroke = Pens.Black;
            Airplane2.Bearing = 0; // Rotation angle
            Airplane2.Fill = new SolidBrush(Color.FromArgb(155, Color.Red)); // Arrow color
            markerOverlay2.Markers.Add(Airplane2);


            //---------------AÑADIR LA CAPA DE MARCADORES AL MAPA
            gMapControl1.Overlays.Add(markerOverlay);
            



        }

        private void lbAlt_Click(object sender, EventArgs e)
        {

        }

        private void btnData_Click(object sender, EventArgs e)
        {
            if (pnlRecordedData.Visible)
            {
                pnlFlightData.Visible = true;
                pnlRecordedData.Visible = false;
            }
        }

        private void btnRecorded_Click(object sender, EventArgs e)
        {
            try
            {

                gMapControl2.Overlays.Add(markerOverlay2);

                if (btnDisconnect.Visible)
                {
                    throw new ApplicationException("The program is recieving data, please disconnect before open 'Recorded Data'");
                }
                else if (pnlFlightData.Visible)
                     {
                        pnlFlightData.Visible = false;
                        pnlRecordedData.Visible = true;
                     }
            }
            catch(ApplicationException error)
            {
                MessageBox.Show(error.Message);
            }
            
        }

        private void btnRec_Click(object sender, EventArgs e)
        {
            timer1.Start();
            //Crear otro hilo en donde se almacenen todos los datos en una tabla en el datatable, para posteriormente al apretar el boton de cargar se importa el datatable al datagridview
            //Solo se debe de cambiar el valor de la variable rec a true para saber que debe de almacenar todos los valores leídos
            pbRec.Visible = true;
            btnRec.Enabled = false;
            btnStop.Enabled = true;
            //rec = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            pbRec.Visible = false;
            btnRec.Enabled = true;
            btnStop.Enabled = false;
            rec = false;
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            try
            {
                if((cmbSpeed.SelectedItem == null) || (cmbPorts.SelectedItem == null))
                {
                    throw new ApplicationException();
                }
                else
                {
                    serialport.BaudRate = int.Parse(cmbSpeed.SelectedItem.ToString());
                    serialport.PortName = cmbPorts.SelectedItem.ToString();

                    btnConnect.Visible = true;
                    btnVerify.Visible = false;
                }
            }
            catch(ApplicationException)
            {
                MessageBox.Show("Baud Rate and/or Serial Ports not selected correctly");
            }
            catch(IOException error)
            {
                MessageBox.Show("Error: " + error.Message);
            }
        }

        private void cmbSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnConnect.Visible = false;
            btnVerify.Visible = true;
        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnConnect.Visible = false;
            btnVerify.Visible = true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                rep = true;
                serialport.Open();
                //del = new ThreadStart(DataRead);
                //ThreadStart del = new ThreadStart(DataRead);
                //Thread line = new Thread(del);
                line = new Thread(DataRead);

                line.Start();

                btnConnect.Visible = false;
                btnDisconnect.Visible = true;
                cmbSpeed.Enabled = false;
                cmbPorts.Enabled = false;
                btnRec.Enabled = true;
                btnRecorded.Enabled = false;
                MessageBox.Show("Successfuly connected");
            }
            catch (ApplicationException)
            {
                btnConnect.Visible = true;
                btnDisconnect.Visible = false;
                cmbSpeed.Enabled = true;
                cmbPorts.Enabled = true;
                btnRec.Enabled = false;
                btnStop.Enabled = false;
                btnRecorded.Enabled = true;
                MessageBox.Show("Can not establish the required connection");
            }
            catch(IOException error)
            {
                MessageBox.Show("Error: " + error.Message);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (pbRec.Visible)
                {
                    throw new ApplicationException("The program is recording flight data, please stop recording before disconnect");
                }
                else
                {
                    //Código de desconección del puerto serial
                    //del.c
                    line.Abort();
                    serialport.Close();
                    rep = false;

                    btnConnect.Visible = true;
                    btnDisconnect.Visible = false;
                    cmbSpeed.Enabled = true;
                    cmbPorts.Enabled = true;
                    btnRec.Enabled = false;
                    btnStop.Enabled = false;
                    btnRecorded.Enabled = true;
                    MessageBox.Show("Successfuly disconnected");
                }
            }
            catch (ApplicationException error)
            {
                btnConnect.Visible = false;
                btnDisconnect.Visible = true;
                cmbSpeed.Enabled = false;
                cmbPorts.Enabled = false;
                btnStop.Enabled = true;
                btnRecorded.Enabled = false;
                MessageBox.Show(error.Message);
            }
            catch(IOException error)
            {
                MessageBox.Show("Error: " + error.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Se guardará la información que se tenga almacenada en las datatables en un texto que se podrá abrir posteriormente y representar en el mismo DataGridView
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            
            dataGridView.Rows[anttrack].Selected = false;
           
            
            timeRec = (int)dataGridView.CurrentRow.Cells["Time [s]"].Value;
            spdRec = (float)dataGridView.CurrentRow.Cells["Speed [ft/s]"].Value;
            altRec = (float)dataGridView.CurrentRow.Cells["Altitude [ft]"].Value;
            LatRec = (double)dataGridView.CurrentRow.Cells["Latitude [°]"].Value;
            LngRec = (double)dataGridView.CurrentRow.Cells["Longitude [°]"].Value;

            Airplane2.Position = new PointLatLng(LatRec, LngRec);
            dataGridView.Rows[trackBar.Value].Selected = true;
            dataGridView.CurrentCell = dataGridView.Rows[trackBar.Value].Cells[0];


            lbTimeRec.Text = timeRec.ToString();
            lbSpdRec.Text = spdRec.ToString();
            lbAltRec.Text = altRec.ToString();
            lbLatRec.Text = LatRec.ToString();
            lbLngRec.Text = LngRec.ToString();
            

            //Poner para que actualice el mapa con las coordenadas de acuerdo al trackBar.SelectedIndex o algo parecido
            //Para buscar en el dataGridView con ese índice los elementos y actualizar el mapa con esas coordenadas, etc.
            anttrack = trackBar.Value;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {

                    dataGridView.DataSource = dt;
                    dataGridView.Rows[timeColonist].DefaultCellStyle.BackColor = Color.Yellow;
                    dataGridView.Rows[timeHabitats].DefaultCellStyle.BackColor = Color.Red;

                    lbColonistTime.Text = timeColonist.ToString();
                    lbHabitatsTime.Text = timeHabitats.ToString();

                     trackBar.SetRange(0, dataGridView.RowCount - 1);
                
            }
            catch(ArgumentOutOfRangeException)
            {
                MessageBox.Show("No data to show");
            }
            
            //Cuando se presione éste boton se realizará algo como trackBar.Maximum = dataGridView.TotalFiles o algo así
            //Para que se añadan exactamente todos los datos que tenemos en la tabla
        }

        private void DataRead()
        {
            while (rep)
            {
                try
                {
                
                    rawdata = serialport.ReadLine();
//                    serialport.
                    string[] data = rawdata.Split(',');

                    if (data.Count() == 13)
                    {

                        #region Variables

                        if (data[0] != "************")
                        {
                            Lat = double.Parse(data[0]);
                            var updateAction = new Action(() => { lbLat.Text = data[0]; });
                            lbLat.Invoke(updateAction);
                            //Agregar al datatable
                            Lng = double.Parse(data[1]);
                            var updateAction2 = new Action(() => { lbLng.Text = data[1]; });
                            lbLng.Invoke(updateAction2);
                            //Agregar al datatable
                            spd = float.Parse(data[2]);
                            var updateAction3 = new Action(() => { lbSpd.Text = data[2]; });
                            lbSpd.Invoke(updateAction3);

                            #region DistanceBetween
                            //Calcular la distancia hasta el punto deseado y decir si deacuerdo al reglamento se puede soltar o no
                            //Actualizando el picturebox de color rojo a verde
                            #endregion
                        }



                        alt = (int)(float.Parse(data[3]));
                        //alt = int.Parse(float.Parse(data[3]));
                        var updateAction4 = new Action(() => { lbAlt.Text = alt.ToString(); });
                        lbAlt.Invoke(updateAction4);
                        //Agregar al datatable

                        if ((data[10] == "0") && (data[11] == "0"))
                        {
                            var updateAction5 = new Action(() => { lbColonist.Text = alt.ToString(); });
                            lbColonist.Invoke(updateAction5);
                            var updateAction6 = new Action(() => { lbPayload.Text = alt.ToString(); });
                            lbPayload.Invoke(updateAction6);
                        }
                        else if ((data[10] == "1") && (data[11] == "0"))
                        {
                            var up1 = new Action(() => { pbColonist.BackColor = Color.Green; });
                            pbColonist.Invoke(up1);
                            var up2 = new Action(() => { lbColonist.BackColor = Color.Green; });
                            lbColonist.Invoke(up2);
                            var up3 = new Action(() => { lbCol1.BackColor = Color.Green; });
                            lbCol1.Invoke(up3);
                            var up4 = new Action(() => { lbCol2.BackColor = Color.Green; });
                            lbCol2.Invoke(up4);
                            var up5 = new Action(() => { lbPayload.Text = alt.ToString(); });
                            lbPayload.Invoke(up5);
                        }
                        else if ((data[10] == "0") && (data[11] == "1"))
                        {
                            var up6 = new Action(() => { pbHabitats.BackColor = Color.Green; });
                            pbHabitats.Invoke(up6);
                            var up7 = new Action(() => { lbPayload.BackColor = Color.Green; });
                            lbPayload.Invoke(up7);
                            var up8 = new Action(() => { lbHab1.BackColor = Color.Green; });
                            lbHab1.Invoke(up8);
                            var up9 = new Action(() => { lbHab2.BackColor = Color.Green; });
                            lbHab2.Invoke(up9);
                            var up10 = new Action(() => { lbColonist.Text = alt.ToString(); });
                            lbColonist.Invoke(up10);
                        }
                        else
                        {
                            var up11 = new Action(() => { pbColonist.BackColor = Color.Green; });
                            pbColonist.Invoke(up11);
                            var up12 = new Action(() => { lbColonist.BackColor = Color.Green; });
                            lbColonist.Invoke(up12);
                            var up13 = new Action(() => { lbCol1.BackColor = Color.Green; });
                            lbCol1.Invoke(up13);
                            var up14 = new Action(() => { lbCol2.BackColor = Color.Green; });
                            lbCol2.Invoke(up14);
                            var up15 = new Action(() => { pbHabitats.BackColor = Color.Green; });
                            pbHabitats.Invoke(up15);
                            var up16 = new Action(() => { lbPayload.BackColor = Color.Green; });
                            lbPayload.Invoke(up16);
                            var up17 = new Action(() => { lbHab1.BackColor = Color.Green; });
                            lbHab1.Invoke(up17);
                            var up18 = new Action(() => { lbHab2.BackColor = Color.Green; });
                            lbHab2.Invoke(up18);
                        }


                        #endregion

                        #region GPS_Update

                        Airplane.Position = new PointLatLng(Lat, Lng);

                        #endregion

                        #region ImgRotation
                        //Rotar imágenes
                        #endregion


                    }
                }
                catch(ArgumentException e)
                {
                    if (serialport.IsOpen)
                    {
                        MessageBox.Show("Error: " + e.Message);
                    }
                    
                }
                catch(IOException e)
                {
                    MessageBox.Show("Error: " + e.Message);
                }
            }
        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                TargetLat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                TargetLng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

                Target.Position = new PointLatLng(TargetLat, TargetLng);
                Target.ToolTipText = string.Format("Target: \n Lat: {0} \n Lng: {1}", TargetLat, TargetLng);

            }
            catch(ApplicationException error)
            {
                MessageBox.Show("Error: " + error.Message);
            }
            
            //Se obtienen los datos de lat y lng del mapa donde el usuario dió doble Clic para establecer el punto de caída
            

        }

        private void btnMapSatellite_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
        }

        private void btnMapDefault_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
        }

        private void btnMapTerrain_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleTerrainMap;
        }

        private void btnDownloadMap_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Now you can drag the map to save the cache data");
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
        }

        private void btnStopDownload_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Recording stopped, now you are in Server mode");
            GMaps.Instance.Mode = AccessMode.ServerOnly;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Al apretar el botón muestra un cuadro de diálogo que diga si está seguro que quiere limpiar los datos antes de guardar
            //Limpiar la variable de tiempo y también el lbTime, lbColonistsTime, lbPayloadTime, PayloadTime, ColonistsTime
            //También limpiar el datagridview y el dt
        }

        private void label23_Click(object sender, EventArgs e) //innecesario
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lbTime.Text = time.ToString();

            if (time % 2 == 0)
            {
                pbRec.Visible = false;

            }
            else
            {
                pbRec.Visible = true;
            }
            dt.Rows.Add(time, lbSpd.Text, lbAlt.Text, lbLat.Text, lbLng.Text);

            if((pbColonist.BackColor == Color.Green) && (boolColonists == false))
            {
                timeColonist = time;
                boolColonists = true;
            }
            if((pbHabitats.BackColor == Color.Green) && (boolHabitats == false))
            {
                timeHabitats = time;
                boolHabitats = true;
            }

            time++;

        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                cmbPorts.Items.Clear();

                String[] ports = SerialPort.GetPortNames();
                Array.Sort(ports);
                cmbPorts.Items.AddRange(ports);
            }
            catch (IOException error)
            {
                MessageBox.Show("Error: " + error.Message);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            gMapControl2.MapProvider = GMapProviders.GoogleMap;
        }

        private void btnSatMap2_Click(object sender, EventArgs e)
        {
            gMapControl2.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
        }

        private void btnTerrainMap2_Click(object sender, EventArgs e)
        {
            gMapControl2.MapProvider = GMapProviders.GoogleHybridMap;
        }

        private void trackBar_Scroll_1(object sender, EventArgs e)
        {
            
        }

        private void pnlFlightData_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlFlightData_Paint_1(object sender, PaintEventArgs e)
        {


        }

    }
}
