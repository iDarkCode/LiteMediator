# LiteMediator

**LiteMediatr** es una implementación ligera del patrón Mediator en C#, diseñada para facilitar la comunicación entre componentes de una aplicación sin necesidad de referencias directas entre ellos. Esta biblioteca es ideal para proyectos medianos que buscan una solución sencilla y eficiente sin depender de bibliotecas externas.

## Características

- **Ligereza**: Sin dependencias externas, lo que facilita su integración en proyectos existentes.
- **Simplicidad**: Fácil de entender y utilizar, siguiendo las prácticas estándar del patrón Mediator.
- **Flexibilidad**: Adecuada para proyectos que requieren una comunicación desacoplada entre componentes.

## Instalación

Puedes agregar LiteMediatr a tu proyecto mediante la inclusión directa del código fuente o empaquetándolo como una biblioteca.

## Uso

A continuación, se muestra un ejemplo básico de cómo utilizar LiteMediatr en una aplicación de consola:

```csharp
// Definición de una solicitud
public class ObtenerSaludoRequest : IRequest<string>
{
    public string Nombre { get; set; }
}

// Implementación del manejador para la solicitud
public class ObtenerSaludoHandler : IRequestHandler<ObtenerSaludoRequest, string>
{
    public string Handle(ObtenerSaludoRequest request)
    {
        return $"¡Hola, {request.Nombre}!";
    }
}

// Configuración y uso del mediador
class Program
{
    static void Main(string[] args)
    {
        // Configuración del mediador y registro de manejadores
        var mediator = new Mediator();
        mediator.Register<ObtenerSaludoRequest, string>(new ObtenerSaludoHandler());

        // Envío de la solicitud y obtención de la respuesta
        var request = new ObtenerSaludoRequest { Nombre = "Carlos" };
        var respuesta = mediator.Send<ObtenerSaludoRequest, string>(request);

        Console.WriteLine(respuesta); // Salida: ¡Hola, Carlos!
    }
}
