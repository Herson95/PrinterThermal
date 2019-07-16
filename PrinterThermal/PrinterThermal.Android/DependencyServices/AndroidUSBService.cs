using PrinterThermal.Droid.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidUSBService))]
namespace PrinterThermal.Droid.DependencyServices
{
    using System;
    using System.Collections.Generic;
    using Android.Hardware.Usb;
    using Android.App;
    using PrinterThermal.DependencyServices;
    using System.Linq;
    using Android.Content;
    using System.Text;
    using System.Threading.Tasks;

    public class AndroidUSBService : IUSBService
    {
        protected string ACTION_USB_PERMISSION = "android.hardware.usb.action.USB_PERMISSION";
        private UsbDevice mDevice;
        UsbDeviceConnection connection;
        //Para una impresora ticketera que imprime a 40 columnas. La variable cortar cortara el texto cuando rebase el limte.
        private int maxCar = 40;
        private int cortar;
        private int Default = 0;

        public List<byte> OutputList { get; set; } = new List<byte>();

        public IList<string> GetDeviceList()
        {
            List<string> lista = new List<string>();
        
            try
            {
                //MainActivity.UsbManager = (UsbManager)MainActivity.Context.GetSystemService(Context.UsbService);

                if (MainActivity.UsbManager.DeviceList.Count != 0)
                {
                    foreach (UsbDevice device in MainActivity.UsbManager.DeviceList.Values)
                    {
                        lista.Add($"USB:{device.SerialNumber}-{device.DeviceId}-{device.DeviceName}-{device.DeviceProtocol}-{device.ProductName} " +
                            $"{device.ProductId} {device.VendorId}");
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return lista;
        }

        public void ConnectAndSend(int productId, int vendorId)
        {
            OutputList = new List<byte>();
            //MainActivity.UsbManager = (UsbManager)MainActivity.Context.GetSystemService(Context.UsbService);

            var matchingDevice = MainActivity.UsbManager.DeviceList.FirstOrDefault(item => item.Value.VendorId == productId && item.Value.VendorId == 5455);
            if (MainActivity.UsbManager.DeviceList.Count == 0)
                throw new Exception("Ningún dispositivo conectado al USB");
            if (matchingDevice.Value == null)
                throw new Exception("Dispositivo no encontrado, intente configurarlo");

            var usbPort = matchingDevice.Key;
            mDevice = matchingDevice.Value;

            if (!MainActivity.UsbManager.HasPermission(mDevice))
            {
                try
                {
                    MainActivity.UsbManager.RequestPermission(mDevice, MainActivity.PendingIntent);
                    throw new Exception("Connetado, intente de nuevo");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            connection = null;
            try
            {
                connection = MainActivity.UsbManager.OpenDevice(mDevice);

                using (var usbInterface = mDevice.GetInterface(0))
                {

                    using (var usbEndpoint = usbInterface.GetEndpoint(0))
                    {
                        connection.ClaimInterface(usbInterface, true);
                        EscInit();
                        SetFont(Default);
                        SetJustification(1);
                        SetBold(true);
                        StoreString("Pay Out\n\n");
                        SetBold(false);
                        SetJustification(0);
                        LineasGuion();
                        TextoExtremos($"Station 5", $"Caja 2");
                        TextoExtremos($"Cashier Name:", $"Victor");
                        TextoExtremos($"Bank Report #:", $"1234");
                        LineasGuion();
                        TextoExtremos($"Pay Out To:", $"Marvin");
                        TextoExtremos($"Issued By Employee:", $"Herson");
                        StoreString("Pay out Details :\n");
                        StoreString($"    Venta de celular\n\n");
                        TextoExtremos($"Pay Out Amount:", $"500.00");
                        Feed(4);
                        SetJustification(1);
                        StoreString("x: _____________________________");
                        Feed(1);
                        StoreString($"{DateTime.UtcNow.ToLocalTime()}");
                        PrintStorage();

                        FeedAndCut(5);
                        connection.BulkTransfer(usbEndpoint, OutputList.ToArray(), OutputList.ToArray().Length, 0);

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

                // Close the connection
                if (connection != null)
                {
                    OutputList = new List<byte>();
                    connection.Close();
                }
            }
        }

        public void CreateConnection()
        {
            MainActivity.CreateConnection();
        }

        #region Commands ESC

        // Abrir caja registradora / cash drawer
        private void OpenCashDrawer()
        {
            OutputList.Add(0x1B);
            OutputList.Add(0x70);
            OutputList.Add(0x0);
            OutputList.Add(0x37);
            OutputList.Add(0x79);

        }

        //Código de inicio esc
        private void EscInit()
        {
            OutputList.Add(0x1B);
            OutputList.Add(0x0F);
        }

        // Save String
        private void PrintString(string str)
        {
            //escInit();
            OutputList.AddRange(Encoding.UTF8.GetBytes(str));
            OutputList.Add(0xA);
        }

        private void SetFontSize(int n)
        {
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("!"));//(86)
            var size = Math.Pow(2, 4) * (n - 1);
            OutputList.Add((byte)size);
        }

        private void SetFont(int n)
        {
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("!"));//(86)
            OutputList.Add((byte)n);
        }

        private void StoreString(string str)
        {
            OutputList.AddRange(Encoding.UTF8.GetBytes(str));
        }

        private void PrintStorage()
        {
            OutputList.Add(0xA);
        }

        //Cortar papel
        private void Cut()
        {
            OutputList.Add(0x1D);
            OutputList.AddRange(Encoding.UTF8.GetBytes("V"));//(86)
            OutputList.Add(48);
            OutputList.Add(0);
        }

        //Imprime n líneas de papel en blanco.
        private void Feed(int feed)
        {
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("d"));
            OutputList.Add((byte)feed);
        }

        //Texto en Negrita
        private void SetBold(bool val)
        {
            var bold = (int)(val ? 1 : 0);
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("E"));
            OutputList.Add((byte)bold);
        }

        //Establece subrayado y grosor.
        /*@param val
         * 0 = no underline.
         * 1 = single weight underline.
         * 2 = double weight underline.
        **/
        private void SetUnderline(int val)
        {
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("-"));
            OutputList.Add((byte)val);
        }

        /*
         * Sets left, center, right justification
         * 
         * @param val
         * 		0 = left justify.
         * 		1 = center justify.
         * 		2 = right justify.
         * */

        private void SetJustification(int val)
        {
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("a"));
            OutputList.Add((byte)val);
        }

        //Integer representing Vertical motion of unit in inches. 0-255
        private void SetLineSpacing(int spacing)
        {

            //function ESC 3
            OutputList.Add(0x1B);
            OutputList.AddRange(Encoding.UTF8.GetBytes("3"));
            OutputList.Add((byte)spacing);

        }

        private void FeedAndCut(int feed)
        {

            Feed(feed);
            Cut();
        }

        private void TexColor(string val)
        {
            var color = (int)(val == "Red" ? 1 : 0);
            OutputList.Add(0x1B);
            OutputList.Add(114);
            OutputList.Add((byte)color);
        }

        private void Character(int c)
        {
            OutputList.Add(0x1B);
            OutputList.Add(0x74);
            OutputList.Add((byte)c);

        }
        #endregion

        #region Methods


        //Metodo para dibujar una linea con asteriscos
        private void LineasAsteriscos()
        {
            string lineasAsterisco = "";
            for (int i = 0; i < maxCar; i++)
            {
                lineasAsterisco += "*";//Agregara un asterisco hasta llegar la numero maximo de caracteres.
            }
            StoreString(lineasAsterisco + "\n"); //Devolvemos la linea con asteriscos
        }

        //Realizamos el mismo procedimiento para dibujar una lineas con el signo igual
        private void LineasIgual()
        {
            string lineasIgual = "";
            for (int i = 0; i < maxCar; i++)
            {
                lineasIgual += "=";//Agregara un igual hasta llegar la numero maximo de caracteres.
            }
            StoreString(lineasIgual + "\n"); //Devolvemos la lienas con iguales
        }

        private void LineasGuion()
        {
            string lineasIgual = "";
            for (int i = 0; i < maxCar; i++)
            {
                lineasIgual += "_";//Agregara un igual hasta llegar la numero maximo de caracteres.
            }
            StoreString(lineasIgual + "\n"); //Devolvemos la lienas con iguales
        }

        //Metodo para poner texto a los extremos
        private void TextoExtremos(string textoIzquierdo, string textoDerecho)
        {
            int number = 21;
            if (maxCar == 40)
            {
                number = 20;
            }
            //variables que utilizaremos
            string textoIzq, textoDer, textoCompleto = "", espacios = "";

            //Si el texto que va a la izquierda es mayor a 18, cortamos el texto.
            if (textoIzquierdo.Length > number)
            {
                cortar = textoIzquierdo.Length - number;
                textoIzq = textoIzquierdo.Remove(number, cortar);
            }
            else
            { textoIzq = textoIzquierdo; }

            textoCompleto = textoIzq;//Agregamos el primer texto.

            if (textoDerecho.Length > number)//Si es mayor a 20 lo cortamos
            {
                cortar = textoDerecho.Length - number;
                textoDer = textoDerecho.Remove(number, cortar);
            }
            else
            { textoDer = textoDerecho; }

            //Obtenemos el numero de espacios restantes para poner textoDerecho al final
            int nroEspacios = maxCar - (textoIzq.Length + textoDer.Length);
            for (int i = 0; i < nroEspacios; i++)
            {
                espacios += " ";//agrega los espacios para poner textoDerecho al final
            }
            textoCompleto += espacios + textoDerecho;//Agregamos el segundo texto con los espacios para alinearlo a la derecha.
            StoreString(textoCompleto + "\n");//agregamos la linea al ticket, al objeto en si.
        }

        //Metodo para poner texto a los extremos
        private void TextoExtremos2(string textoIzquierdo, string textoDerecho)
        {
            int number = 35;
            if (maxCar == 40)
            {
                number = 33;
            }
            //variables que utilizaremos
            string textoIzq, textoDer, textoCompleto = "", espacios = "";

            //Si el texto que va a la izquierda es mayor a 18, cortamos el texto.
            if (textoIzquierdo.Length > number)
            {
                cortar = textoIzquierdo.Length - number;
                textoIzq = textoIzquierdo.Remove(number, cortar);
            }
            else
            { textoIzq = textoIzquierdo; }

            textoCompleto = textoIzq;//Agregamos el primer texto.

            if (textoDerecho.Length > 7)//Si es mayor a 20 lo cortamos
            {
                cortar = textoDerecho.Length - 7;
                textoDer = textoDerecho.Remove(7, cortar);
            }
            else
            { textoDer = textoDerecho; }

            //Obtenemos el numero de espacios restantes para poner textoDerecho al final
            int nroEspacios = maxCar - (textoIzq.Length + textoDer.Length);
            for (int i = 0; i < nroEspacios; i++)
            {
                espacios += " ";//agrega los espacios para poner textoDerecho al final
            }
            textoCompleto += espacios + textoDerecho;//Agregamos el segundo texto con los espacios para alinearlo a la derecha.
            StoreString(textoCompleto + "\n");//agregamos la linea al ticket, al objeto en si.
        }


        //Metodo para agreagar articulos al ticket de venta
        private void AgregaArticulo(string articulo, int cant, decimal precio, decimal importe)
        {


            //Valida que cant precio e importe esten dentro del rango.
            if (cant.ToString().Length <= 5 && precio.ToString().Length <= 7 && importe.ToString().Length <= 8)
            {
                string elemento = "", espacios = "";
                bool bandera = false;//Indicara si es la primera linea que se escribe cuando bajemos a la segunda si el nombre del articulo no entra en la primera linea
                int nroEspacios = 0;

                //Si el nombre o descripcion del articulo es mayor a 20, bajar a la siguiente linea
                if (articulo.Length > 20)
                {
                    //Colocar la cantidad a la derecha.
                    nroEspacios = (5 - cant.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";//Generamos los espacios necesarios para alinear a la derecha
                    }
                    elemento += espacios + cant.ToString();//agregamos la cantidad con los espacios

                    //Colocar el precio a la derecha.
                    nroEspacios = (7 - precio.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";//Genera los espacios
                    }
                    //el operador += indica que agregar mas cadenas a lo que ya existe.
                    elemento += espacios + precio.ToString();//Agregamos el precio a la variable elemento

                    //Colocar el importe a la derecha.
                    nroEspacios = (8 - importe.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elemento += espacios + importe.ToString() + "\n";//Agregamos el importe alineado a la derecha

                    int caracterActual = 0;//Indicara en que caracter se quedo al bajae a la siguiente linea

                    //Por cada 20 caracteres se agregara una linea siguiente
                    for (int longitudTexto = articulo.Length; longitudTexto > 20; longitudTexto -= 20)
                    {
                        if (bandera == false)//si es false o la primera linea en recorrerer, continuar...
                        {

                            //agregamos los primeros 20 caracteres del nombre del articulos, mas lo que ya tiene la variable elemento
                            StoreString(articulo.Substring(caracterActual, 20) + elemento);
                            bandera = true;//cambiamos su valor a verdadero
                        }
                        else
                            StoreString(articulo.Substring(caracterActual, 20) + "\n");//Solo agrega el nombre del articulo

                        caracterActual += 20;//incrementa en 20 el valor de la variable caracterActual
                    }
                    //Agrega el resto del fragmento del  nombre del articulo
                    StoreString(articulo.Substring(caracterActual, articulo.Length - caracterActual) + "\n");

                }
                else //Si no es mayor solo agregarlo, sin dar saltos de lineas
                {
                    for (int i = 0; i < (20 - articulo.Length); i++)
                    {
                        espacios += " "; //Agrega espacios para completar los 20 caracteres
                    }
                    elemento = articulo + espacios;

                    //Colocar la cantidad a la derecha.
                    nroEspacios = (5 - cant.ToString().Length);// +(20 - elemento.Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elemento += espacios + cant.ToString();

                    //Colocar el precio a la derecha.
                    nroEspacios = (7 - precio.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elemento += espacios + precio.ToString();

                    //Colocar el importe a la derecha.
                    nroEspacios = (8 - importe.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elemento += espacios + importe.ToString() + "\n";

                    StoreString(elemento);//Agregamos todo el elemento: nombre del articulo, cant, precio, importe.
                }
            }
            else
            {
                StoreString("Los valores ingresados para esta fila");
                StoreString("superan las columnas soportdas por éste.");
                throw new Exception("Los valores ingresados para algunas filas del ticket\nsuperan las columnas soportdas por éste.");
            }
        }


        private void AgregaArticulo1(string cant, string articulo, string precio)
        {
            int number = 26;
            int number2 = 30;
            if (maxCar == 40)
            {
                number = 24;
                number2 = 28;
            }

            if (cant == "0")
            {
                cant = "";
            }
            //Valida que cant precio e importe esten dentro del rango.
            if (cant.ToString().Length <= 5 && precio.ToString().Length <= 7)
            {
                string elemento = "", espacios = "";
                bool bandera = false;//Indicara si es la primera linea que se escribe cuando bajemos a la segunda si el nombre del articulo no entra en la primera linea
                int nroEspacios = 0;

                //Si el nombre o descripcion del articulo es mayor a 20, bajar a la siguiente linea
                if (articulo.Length > number)
                {
                    //Colocar el precio a la derecha.
                    nroEspacios = (7 - precio.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";//Genera los espacios
                    }
                    //el operador += indica que agregar mas cadenas a lo que ya existe.
                    elemento = espacios + precio.ToString();

                    int caracterActual = 0;//Indicara en que caracter se quedo al bajae a la siguiente linea

                    //Por cada 20 caracteres se agregara una linea siguiente
                    for (int longitudTexto = articulo.Length; longitudTexto > number2; longitudTexto -= number2)
                    {
                        if (bandera == false)//si es false o la primera linea en recorrerer, continuar...
                        {
                            nroEspacios = (5 - cant.ToString().Length);
                            espacios = "";
                            for (int i = 0; i < nroEspacios; i++)
                            {
                                espacios += " ";//Generamos los espacios necesarios para alinear a la derecha
                            }
                            //agregamos los primeros 20 caracteres del nombre del articulos, mas lo que ya tiene la variable elemento
                            StoreString(cant.ToString() + espacios + articulo.Substring(caracterActual, number2) + elemento + "\n");
                            bandera = true;//cambiamos su valor a verdadero
                        }
                        else
                            StoreString("     " + articulo.Substring(caracterActual, number2) + "\n");//Solo agrega el nombre del articulo

                        caracterActual += number2;//incrementa en 20 el valor de la variable caracterActual
                    }
                    espacios = "";
                    for (int i = 0; i < 5; i++)
                    {
                        espacios += " ";//Generamos los espacios necesarios para alinear a la derecha
                    }
                    //Agrega el resto del fragmento del  nombre del articulo
                    StoreString(espacios + articulo.Substring(caracterActual, articulo.Length - caracterActual) + "\n");

                }
                else //Si no es mayor solo agregarlo, sin dar saltos de lineas
                {
                    nroEspacios = (5 - cant.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";//Generamos los espacios necesarios para alinear a la derecha
                    }
                    elemento = cant + espacios;

                    espacios = "";
                    for (int i = 0; i < (number2 - articulo.Length); i++)
                    {
                        espacios += " "; //Agrega espacios para completar los 20 caracteres
                    }
                    elemento += articulo + espacios;

                    //Colocar el precio a la derecha.
                    nroEspacios = (7 - precio.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elemento += espacios + precio.ToString() + "\n";

                    StoreString(elemento);//Agregamos todo el elemento: nombre del articulo, cant, precio, importe.
                }
            }
            else
            {
                StoreString("Los valores ingresados para esta fila\n");
                StoreString("superan las columnas soportdas por éste.\n");
                throw new Exception("Los valores ingresados para algunas filas del ticket\nsuperan las columnas soportdas por éste.");
            }
        }

        private void AgregarTotales(decimal sub, decimal tax, decimal total)
        {
            string elemento = "", espacios = "";
            int nroEspacios = 0;

            nroEspacios = (13 - sub.ToString("F").Length);
            espacios = "";
            for (int i = 0; i < nroEspacios; i++)
            {
                espacios += " ";
            }
            elemento = sub.ToString("F") + espacios;

            espacios = "";
            for (int i = 0; i < (13 - tax.ToString("F").Length); i++)
            {
                espacios += " ";
            }
            elemento += tax.ToString("F") + espacios;


            nroEspacios = (15 - total.ToString("F").Length);
            espacios = "";
            for (int i = 0; i < nroEspacios; i++)
            {
                espacios += " ";
            }
            elemento += "$" + total.ToString("F") + espacios + "\n";

            StoreString(elemento);
        }


        private void ItemsNamePrice(string name, string precio)
        {
            string elemento = "", espacios = "";
            bool bandera = false;//Indicara si es la primera linea que se escribe cuando bajemos a la segunda si el nombre del articulo no entra en la primera linea
            int nroEspacios = 0;

            //Si el nombre o descripcion del articulo es mayor a 20, bajar a la siguiente linea
            if (name.Length > 35)
            {
                //Colocar el precio a la derecha.
                nroEspacios = (7 - precio.ToString().Length);
                espacios = "";
                for (int i = 0; i < nroEspacios; i++)
                {
                    espacios += " ";//Genera los espacios
                }
                //el operador += indica que agregar mas cadenas a lo que ya existe.
                elemento = espacios + precio.ToString();

                int caracterActual = 0;//Indicara en que caracter se quedo al bajae a la siguiente linea

                //Por cada 20 caracteres se agregara una linea siguiente
                for (int longitudTexto = name.Length; longitudTexto > 35; longitudTexto -= 35)
                {
                    if (bandera == false)//si es false o la primera linea en recorrerer, continuar...
                    {
                        StoreString(name.Substring(caracterActual, 35) + elemento + "\n");
                        bandera = true;//cambiamos su valor a verdadero
                    }
                    else
                        StoreString(name.Substring(caracterActual, 35) + "\n");//Solo agrega el nombre del articulo

                    caracterActual += 35;//incrementa en 20 el valor de la variable caracterActual
                }
                espacios = "";
                for (int i = 0; i < 7; i++)
                {
                    espacios += " ";//Generamos los espacios necesarios para alinear a la derecha
                }
                //Agrega el resto del fragmento del  nombre del articulo
                StoreString(espacios + name.Substring(caracterActual, name.Length - caracterActual) + "\n");

            }

            else //Si no es mayor solo agregarlo, sin dar saltos de lineas
            {
                espacios = "";
                for (int i = 0; i < (35 - name.Length); i++)
                {
                    espacios += " "; //Agrega espacios para completar los 20 caracteres
                }
                elemento += name + espacios;

                //Colocar el precio a la derecha.
                nroEspacios = (5 - precio.ToString().Length);
                espacios = "";
                for (int i = 0; i < nroEspacios; i++)
                {
                    espacios += " ";
                }
                elemento += espacios + precio.ToString() + "\n";

                StoreString(elemento);//Agregamos todo el elemento: nombre del articulo, cant, precio, importe.
            }
        }

        private void ProductKitchenBar(string cant, string name)
        {
            string elemento = "", espacios = "";
            bool bandera = false;//Indicara si es la primera linea que se escribe cuando bajemos a la segunda si el nombre del articulo no entra en la primera linea
            int nroEspacios = 0;

            //Si el nombre o descripcion del articulo es mayor a 20, bajar a la siguiente linea
            if (name.Length > 35)
            {
                //Colocar el precio a la derecha.
                nroEspacios = (5 - cant.ToString().Length);
                espacios = "";
                for (int i = 0; i < nroEspacios; i++)
                {
                    espacios += " ";//Genera los espacios
                }
                //el operador += indica que agregar mas cadenas a lo que ya existe.
                elemento = cant.ToString() + espacios;

                int caracterActual = 0;//Indicara en que caracter se quedo al bajae a la siguiente linea

                //Por cada 20 caracteres se agregara una linea siguiente
                for (int longitudTexto = name.Length; longitudTexto > 35; longitudTexto -= 35)
                {
                    if (bandera == false)//si es false o la primera linea en recorrerer, continuar...
                    {
                        StoreString(elemento + name.Substring(caracterActual, 35) + "\n");
                        bandera = true;//cambiamos su valor a verdadero
                    }
                    else
                        StoreString("     " + name.Substring(caracterActual, 35) + "\n");//Solo agrega el nombre del articulo

                    caracterActual += 35;//incrementa en 20 el valor de la variable caracterActual
                }
                espacios = "";
                for (int i = 0; i < 5; i++)
                {
                    espacios += " ";//Generamos los espacios necesarios para alinear a la derecha
                }
                //Agrega el resto del fragmento del  nombre del articulo
                StoreString(espacios + name.Substring(caracterActual, name.Length - caracterActual) + "\n");

            }

            else //Si no es mayor solo agregarlo, sin dar saltos de lineas
            {
                espacios = "";
                for (int i = 0; i < (35 - name.Length); i++)
                {
                    espacios += " "; //Agrega espacios para completar los 20 caracteres
                }
                elemento += espacios + name;

                //Colocar el precio a la derecha.
                nroEspacios = (5 - cant.ToString().Length);
                espacios = "";
                for (int i = 0; i < nroEspacios; i++)
                {
                    espacios += " ";
                }
                elemento += cant.ToString() + espacios + "\n";

                StoreString(elemento);//Agregamos todo el elemento: nombre del articulo, cant, precio, importe.
            }
        }

        private void Character(char item)
        {

            if (item == 'á')
            {
                Character(0xA0);
            }
            if (item == 'í')
            {
                Character(0xA1);
            }
            if (item == 'ó')
            {
                Character(0xA2);
            }
            if (item == 'ú')
            {
                Character(0xA3);
            }
            if (item == 'ñ')
            {
                Character(0xA4);
            }
            if (item == 'Ñ')
            {
                Character(0xA5);
            }
        }
        #endregion

    }
}