
using PrinterThermal.Droid.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidPrinterService))]
namespace PrinterThermal.Droid.DependencyServices
{
    using System.Threading.Tasks;
    using Java.IO;
    using Java.Net;
    using System;
    using Console = System.Console;
    using System.Linq;
    using Android.Bluetooth;
    using Java.Util;
    using Android.Hardware.Usb;
    using Android.App;
    using PrinterThermal.DependencyServices;
    using Android.Content;

    public class AndroidPrinterService : IPrinterService
    {
        #region Properties
        protected static string ACTION_USB_PERMISSION = "com.printerApp.USB_PERMISSION";
        Socket Socket;
        PrintWriter pw;

        //Para una impresora ticketera que imprime a 40 columnas. La variable cortar cortara el texto cuando rebase el limte.
        private int maxCar = 40;
        private int cortar;
        private int Default = 0;


        public BluetoothSocket BluetoothSocket { get; set; }
        
        #endregion

        public async Task Test(string ip, int port, string addressBluetooth, int fontSize = 0, string type = "1")
        {
            try
            {
                if (type == "1")
                {
                    Socket = new Socket();
                    await Socket?.ConnectAsync(new InetSocketAddress(ip, port), 1000);
                    pw = new PrintWriter(Socket.OutputStream, true);
                }
                else if (type == "2")
                {
                  
                    var address = addressBluetooth.Split(" ").LastOrDefault();
                    var bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                    if (!bluetoothAdapter.IsEnabled)
                    {
                        Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                        MainActivity.Context.StartActivity(enableBtIntent);
                        await Task.Delay(6000);
                        if (!bluetoothAdapter.IsEnabled)
                        {
                            throw new Exception("Bluetooth adapter is not enabled.");
                        }
                       
                    }
                    var device = (from bd in bluetoothAdapter?.BondedDevices
                                  where bd?.Address == address
                                  select bd).FirstOrDefault();

                    if (device==null)
                    {
                        throw new Exception("Bluetooth without connection");
                    }
                    else
                    {
                        BluetoothSocket = device?.CreateRfcommSocketToServiceRecord(AndroidBlueToothService.UUID);
                        try
                        {
                            await BluetoothSocket?.ConnectAsync();
                        }
                        catch
                        {
                            throw new Exception("Connection denied.");
                        }
                      
                        pw = new PrintWriter(BluetoothSocket.OutputStream, true);
                    }
                   
                }

                if (type != "3")
                {
                    await Task.Run(() =>
                    {

                        EscInit();
                        SetJustification(1);
                        SetFontSize(fontSize);
                        SetBold(true);
                        StoreString("PRINTER TEST \n\n");
                        SetBold(false);
                        SetFont(Default);
                        PrintStorage();

                        FeedAndCut(4);

                        pw.Flush();
                        pw.Close();
                        pw.Dispose();
                        if (type == "1")
                        {
                            Socket.Close();
                            Socket.Dispose();
                        }
                        else
                        {
                            BluetoothSocket.Close();
                            BluetoothSocket.Dispose();
                        }

                    });
                }


            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketError: " + ex.Message);
            }
        }

        #region Commands ESC

        // Abrir caja registradora / cash drawer
        void OpenCashDrawer()
        {
            pw.Write(0x1B);
            pw.Write(0x70);
            pw.Write(0x0);
            pw.Write(0x37);
            pw.Write(0x79);

        }

        //Código de inicio esc
        void EscInit()
        {
            pw.Write(0x1B);
            pw.Write(0x0F);
        }

        // Save String
        void PrintString(string str)
        {
            //escInit();
            pw.Write(str);
            pw.Write(0xA);
        }

        void SetFontSize(int n)
        {
            pw.Write(0x1B);
            pw.Write("!");//(86)
            var size = Math.Pow(2, 4) * (n - 1);
            pw.Write((char)size);
        }

        void SetFont(int n)
        {
            pw.Write(0x1B);
            pw.Write("!");//(86)
            pw.Write(n);
        }

        void StoreString(string str)
        {
            pw.Write(str);
        }

        void PrintStorage()
        {
            pw.Write(0xA);
        }

        //Cortar papel
        void Cut()
        {
            pw.Write(0x1D);
            pw.Write("V");//(86)
            pw.Write(48);
            pw.Write(0);
        }

        //Imprime n líneas de papel en blanco.
        void Feed(int feed)
        {
            //escInit();
            pw.Write(0x1B);
            pw.Write("d");
            pw.Write(feed);
        }

        //Texto en Negrita
        void SetBold(bool val)
        {
            pw.Write(0x1B);
            pw.Write("E");
            pw.Write((int)(val ? 1 : 0));
        }

        //Establece subrayado y grosor.
        /*@param val
         * 0 = no underline.
         * 1 = single weight underline.
         * 2 = double weight underline.
        **/
        void SetUnderline(int val)
        {
            pw.Write(0x1B);
            pw.Write("-");
            pw.Write(val);
        }

        /*
         * Sets left, center, right justification
         * 
         * @param val
         * 		0 = left justify.
         * 		1 = center justify.
         * 		2 = right justify.
         * */

        void SetJustification(int val)
        {
            pw.Write(0x1B);
            pw.Write("a");
            pw.Write(val);
        }

        //Integer representing Vertical motion of unit in inches. 0-255
        void SetLineSpacing(int spacing)
        {

            //function ESC 3
            pw.Write(0x1B);
            pw.Write("3");
            pw.Write(spacing);

        }

        void FeedAndCut(int feed)
        {

            Feed(feed);
            Cut();
        }

        void TexColor(string val)
        {
            pw.Write(0x1B);
            pw.Write(114);
            pw.Write((int)(val == "Red" ? 1 : 0));
        }

        void Character(int c)
        {
            pw.Write(0x1B);
            pw.Write(0x74);
            pw.Write(c);

        }
        #endregion

        #region Methods


        //Metodo para dibujar una linea con asteriscos
        void LineasAsteriscos()
        {
            string lineasAsterisco = "";
            for (int i = 0; i < maxCar; i++)
            {
                lineasAsterisco += "*";//Agregara un asterisco hasta llegar la numero maximo de caracteres.
            }
            StoreString(lineasAsterisco + "\n"); //Devolvemos la linea con asteriscos
        }

        //Realizamos el mismo procedimiento para dibujar una lineas con el signo igual
        void LineasIgual()
        {
            string lineasIgual = "";
            for (int i = 0; i < maxCar; i++)
            {
                lineasIgual += "=";//Agregara un igual hasta llegar la numero maximo de caracteres.
            }
            StoreString(lineasIgual + "\n"); //Devolvemos la lienas con iguales
        }

        void LineasGuion()
        {
            string lineasIgual = "";
            for (int i = 0; i < maxCar; i++)
            {
                lineasIgual += "_";//Agregara un igual hasta llegar la numero maximo de caracteres.
            }
            StoreString(lineasIgual + "\n"); //Devolvemos la lienas con iguales
        }

        //Metodo para poner texto a los extremos
        void TextoExtremos(string textoIzquierdo, string textoDerecho)
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
        void TextoExtremos2(string textoIzquierdo, string textoDerecho)
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
        void AgregaArticulo(string articulo, int cant, decimal precio, decimal importe)
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


        void AgregaArticulo1(string cant, string articulo, string precio)
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

        void AgregarTotales(decimal sub, decimal tax, decimal total)
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