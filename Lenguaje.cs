using System;
using System.Collections.Generic;
using System.Text;

// Requerimiento 1: Implementar las secuencias de escape: \n, \t cuando se imprime una cadena y 
//                  eliminar las dobles comillas. 
// Requerimiento 2: Levantar excepciones en la clase Stack.
// Requerimiento 3: Agregar el tipo de dato en el Inserta de ListaVariables.
// Requerimiento 4: Validar existencia o duplicidad de variables. Mensaje de error:
//                  "Error de sintaxis: La variable (x26) no ha sido declarada."
//                  "Error de sintaxis: La variables (x26) está duplicada." 
// Requerimiento 5: Modificar el valor de la variable o constante al momento de su declaración.

namespace Sintaxis3
{
    class Lenguaje: Sintaxis
    {
        Stack s;
        ListaVariables l;
        public Lenguaje()
        {            
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre): base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        // Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {            
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);

                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }

                match(">");

                Libreria();
            }
        }

        // Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");

            BloqueInstrucciones(true);            
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {
            match(clasificaciones.inicioBloque);

            Instrucciones(ejecuta);

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(string Almacenar)
        {          
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad

            if (!l.Existe(nombre))
            {
                if (Almacenar == "int")
                {
                    l.Inserta(nombre, Variable.tipo.INT);
                }
                else if (Almacenar == "string")
                {
                    l.Inserta(nombre, Variable.tipo.STRING);
                }
                else if (Almacenar == "float")
                {
                    l.Inserta(nombre, Variable.tipo.FLOAT);
                }
                else if (Almacenar == "char")
                {
                    l.Inserta(nombre, Variable.tipo.CHAR);
                }
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable Duplicada (" + nombre + ")" + "(" + linea + "," + caracter + ")");
            }
                string valor = "";

            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);
                

                if (getClasificacion() == clasificaciones.cadena)
                {           
                    valor = getContenido();         
                    match(clasificaciones.cadena);                    
                }
                else 
                {                    
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                }                 
            }
                l.setValor(nombre, valor);

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(Almacenar);
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            string Almacenar = getContenido();
            match(clasificaciones.tipoDato);
            Lista_IDs(Almacenar, ejecuta);
            match(clasificaciones.finSentencia);           
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DoWhile(ejecuta);
            }
            else if (getContenido() == "while")
            {
                While(ejecuta);
            }
            else if (getContenido() == "for")
            {
                For(ejecuta);
            }
            else if (getContenido() == "if")
            {
                If(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                // Requerimiento 5
                match("cin"); 
                match(clasificaciones.flujoEntrada);
                string nombre=getContenido();
                if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                    if (ejecuta)
                    {
                        string valor = Console.ReadLine();
                        l.setValor(nombre, valor);
                    }
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
            }            
            else
            {
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia

                if(!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                match(clasificaciones.asignacion);

                string valor;

                if (getClasificacion() == clasificaciones.cadena)
                {           
                    valor = getContenido();         
                    match(clasificaciones.cadena);                    
                }
                else
                {                    
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                }                

                if (ejecuta)
                {
                    l.setValor(nombre, valor);
                }

                match(clasificaciones.finSentencia);
            }
        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(bool ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante()
        {
            match("const");
            string Almacenar = getContenido();
            match(clasificaciones.tipoDato);
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad

            if (!l.Existe(nombre))
            {
                if (Almacenar == "int")
                {
                    l.Inserta(nombre, Variable.tipo.INT, true);
                }
                else if (Almacenar == "string")
                {
                    l.Inserta(nombre, Variable.tipo.STRING, true);
                }
                else if (Almacenar == "float")
                {
                    l.Inserta(nombre, Variable.tipo.FLOAT, true);
                }
                else if (Almacenar == "char")
                {
                    l.Inserta(nombre, Variable.tipo.CHAR, true);
                }
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }

            match(clasificaciones.asignacion);

            string valor;

                if (getClasificacion() == clasificaciones.cadena)
                {           
                    valor = getContenido();         
                    match(clasificaciones.cadena);                    
                }
                else
                {                    
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                }                

            l.setValor(nombre, valor);
         
            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida()
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                Console.Write(getContenido());
                match(clasificaciones.numero); 
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {                                
                string SecuenciaEscape = getContenido();
                if (SecuenciaEscape.Contains("\""))
                {
                    SecuenciaEscape = SecuenciaEscape.Replace("\"", "");
                }

                if (SecuenciaEscape.Contains("\\n"))
                {
                    SecuenciaEscape = SecuenciaEscape.Replace("\\n", "\n"); 
                }

                if (SecuenciaEscape.Contains("\\t"))
                {
                    SecuenciaEscape = SecuenciaEscape.Replace("\\t", "\t");
                }

                Console.Write(SecuenciaEscape);
                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    Console.Write(l.getValor(nombre));
                    match(clasificaciones.identificador); // Validar existencia 
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                               
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida();
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If()
        {
            match("if");
            match("(");
            bool ejecuta = Condicion();
            match(")");
            BloqueInstrucciones(ejecuta);

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(ejecuta);
            }
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            string Operador = get.getContenido();
            match(clasificaciones.operadorRelacional);
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);

            switch(operador)
            {
                case ">"
                    return n1 > n2;

                case ">="
                    return n1 > n2;

                case "<"
                    return n1 > n2;

                case "<="
                    return n1 > n2;

                case "=="
                    return n1 > n2;

                default: 
                    return n1 > n2;
            }
            
        }

        // x26 = (3+5)*8-(10-4)/2;
        // Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                string operador = getContenido();                              
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);  
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "+":
                        s.push(e2+e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2-e1, bitacora, linea, caracter);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        // Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                string operador = getContenido();                
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter); 
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "*":
                        s.push(e2*e1, bitacora, linea, caracter);                        
                        break;
                    case "/":
                        s.push(e2/e1, bitacora, linea, caracter);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        // Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                string nombre = getContenido();
                s.display(bitacora);
                match(clasificaciones.identificador); // Validar existencia

                if(!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                
                s.push(float.Parse(l.getValor(nombre)), bitacora, linea, caracter);
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                // Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora);
                match(clasificaciones.numero);
            }
            else
            {
                match("(");
                Expresion();
                match(")");
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For()
        {
            match("for");

            match("(");

                string nombre = getContenido();
                if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
    
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);
            Condicion();
            match(clasificaciones.finSentencia);
            nombre = getContenido();

            if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }

            match(clasificaciones.incrementoTermino);

            match(")");

            BloqueInstrucciones();
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While()
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones();
        }
        
        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile()
        {
            match("do");

            BloqueInstrucciones();

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        // x26 = (3 + 5) * 8 - (10 - 4) / 2 
        // x26 = 3 + 5 * 8 - 10 - 4 / 2
        // x26 = 3 5 + 8 * 10 4 - 2 / -
    }
}
