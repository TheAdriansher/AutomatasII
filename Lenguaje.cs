using System;
using System.Collections.Generic;
using System.Text;

// Requerimiento 1: Implementar el not en el if.
// Requerimiento 2: Validar la asignacion de strings en Intruccion.
// Requerimiento 3: Implementar la comparacion de Tipos de datos en Lista_IDs.
// Requerimiento 4: Validar los tipos de datos en la asignacion del cin.
// Requerimiento 5: Implementar el cast.

namespace Sintaxis3
{
    class Lenguaje: Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo maxBytes;
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
        private void Lista_IDs(string Almacenar, bool ejecuta)
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
                    if (Almacenar == "string")
                    {
                        match(clasificaciones.cadena);
                    }
                    else
                    {
                        throw new Error(bitacora, "Error Semantico: No se puede asignar un STRING a un  (" + Almacenar + ") " + "(" + linea + ", " + caracter + ")"); 
                    }                     
                }
                else 
                {   
                    // Requerimiento 3.                 
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                }                 
            }
            
            if (ejecuta)
            {
                l.setValor(nombre, valor);
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(Almacenar, ejecuta);
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
                // Requerimiento 4
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
                // Requerimiento 2.
                if (getClasificacion() == clasificaciones.cadena)
                {           
                    valor = getContenido();         
                    match(clasificaciones.cadena);                    
                }
                else
                {        
                    // Requerimiento 3.
                    maxBytes = Variable.tipo.CHAR;     
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    
                    if (tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }

                    if (maxBytes > l.getTipoDato(nombre))
                    {
                        throw new Error(bitacora, "Error semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
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
            Instruccion(ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {
            match("const");

            string Almacenar = getContenido();
            match(clasificaciones.tipoDato);

            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad

            if (!l.Existe(nombre) && ejecuta)
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
                    maxBytes =  Variable.tipo.CHAR;           
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                }                
            if (ejecuta)
            {
                l.setValor(nombre, valor);
            }
            
            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if (ejecuta)
                {
                    Console.Write(getContenido());
                }

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
                if (ejecuta)
                {
                    Console.Write(SecuenciaEscape);
                }
                
                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    if (ejecuta)
                    {
                        Console.Write(l.getValor(nombre));
                    }
                    
                    match(clasificaciones.identificador); // Validar existencia 
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variale no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                               
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If(bool ejecuta2)
        {
            match("if");
            match("(");
            bool ejecuta = Condicion();
            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);
            

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(!ejecuta && ejecuta2);
            }
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            maxBytes =  Variable.tipo.CHAR;  
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            maxBytes =  Variable.tipo.CHAR;  
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);

            switch(operador)
            {
                case ">":
                    return n1 > n2;

                case ">=":
                    return n1 >= n2;

                case "<":
                    return n1 < n2;

                case "<=":
                    return n1 <= n2;

                case "==":
                    return n1 == n2;

                default:
                    return n1 != n2;
            
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
                
                if (l.getTipoDato(nombre) > maxBytes)
                {
                    maxBytes = l.getTipoDato(nombre);
                }

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
                
                if (tipoDatoExpresion(float.Parse(getContenido())) > maxBytes)
                {
                    maxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                }

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
        private void For(bool ejecuta)
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

            BloqueInstrucciones(ejecuta);
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones(ejecuta);
        }
        
        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile(bool ejecuta)
        {
            match("do");

            BloqueInstrucciones(ejecuta);

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        private Variable.tipo tipoDatoExpresion(float valor) 
        {
            if (valor % 1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if (valor < 255)
            {
                return Variable.tipo.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.tipo.INT;
            }
            
            return Variable.tipo.FLOAT;
        }
    }
}
