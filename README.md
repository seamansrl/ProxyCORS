# Introducción

Hoy es ese dÍa en donde terminaste un proyecto en JavaScript, Node.JS o TypeCode el cual conecta con una API externa y cuando lo querés probar no funcionan nada. Desesperado empezás a buscar fallas en el código, pero no encontras razón alguna hasta que ves el siguiente cartel:


Access to XMLHttpRequest at 'http://……….' from origin 'null' has been blocked by CORS policy: Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource. 

Pero... ¿Qué pasó acá? ¿Qué es ésto?, te preguntás.

Cuando se abre una página web, cargar datos de servidores ajenos está, en teoría, estrictamente prohibido. Sin embargo, puede haber excepciones: si los administradores de ambas webs han acordado trabajar juntos, no hay por qué evitar el intercambio. En estos casos, el llamado cross-origin resource sharing (CORS) regula la colaboración. 

# ¿Qué son las CORS?

La same-origin policy (SOP o política de seguridad del mismo origen) prohíbe que se carguen datos de servidores ajenos al acceder a una página web. Todos los datos deben provenir de la misma fuente, es decir, corresponder al mismo servidor. Se trata de una medida de seguridad, ya que JavaScript y CSS podrían cargar, sin que el usuario lo supiese, contenido de otros servidores (y, con este, también contenido malicioso). Tales intentos son denominados “cross-origin requests”. Si, por el contrario, ambos administradores web saben del intercambio de contenido y lo aprueban, no tiene sentido impedir este proceso. El servidor solicitado (es decir, aquel del que se quiere cargar contenido) puede permitir entonces el acceso mediante cross-origin resource sharing, en castellano, intercambio de recursos de origen cruzado.
Este permiso se da, no obstante, únicamente a clientes concretos, es decir, el CORS no es un comodín para realizar cualquier cross-origin request. En lugar de eso, el segundo servidor permite al primero un acceso exclusivo mediante una cabecera HTTP. En dicha cabecera de la respuesta HTTP está indicado específicamente qué servidores pueden cargar datos y ponerlos a disposición del usuario. El acceso generalizado a todos los clientes se permite únicamente mediante una “wildcard” o certificado comodín. Esta solución, sin embargo, solo es conveniente para servidores cuyo contenido debe estar a disposición del público general, como es el caso, por ejemplo, de las tipografías web.
Si todo sale bien, el usuario no se percatará en absoluto del intercambio entre ambos servidores. Todos los navegadores actuales soportan el CORS, y el envío de solicitudes y respuestas sucede rápidamente al solicitar una página web sin que el usuario lo note.


# Estructura de la CORS header

De acuerdo con la política de seguridad del mismo origen, en una conexión entre servidores, los datos referentes al origen se componen de tres elementos: host, puerto y protocolo. De este modo, y tomando el ejemplo de la imagen, la directriz prohíbe que ’https://example.com’ acceda a ’http://example.com’ o a ’https://example.org’. En el primer caso, el protocolo no es el mismo y, en el segundo, los datos de host no coinciden.
Una petición de origen cruzado es, en teoría, una petición HTTP. Los métodos específicos no suelen dar problemas. GET y HEAD no pueden alterar datos y, por lo tanto, no suelen considerarse como un riesgo para la seguridad. No se puede decir lo mismo de PATCH, PUT o DELETE: con ellos sí se puede llevar a cabo acciones maliciosas, por lo que en estos casos también hay que activar el cross-origin resource sharing, ya que CORS no solo puede contener información sobre el origen permitido, sino también acerca de qué peticiones HTTP están permitidas por la fuente.
Si se trata de métodos HTTP de seguridad, el cliente envía en primer lugar una solicitud preflight(preflight request) en la que solo se indica qué método HTTP se piensa transmitir al servidor a continuación y se pregunta si la solicitud es considerada segura. Para ello, se usa la cabecera OPTIONS (OPTIONS header). Una vez se haya recibido una respuesta positiva, ya se puede realizar la solicitud propiamente dicha.
Existen diferentes cabeceras o CORS headers y cada una aborda un aspecto distinto. Ya hemos mencionado dos cabeceras importantes para identificar orígenes seguros y métodos permitidos, pero hay más:
-	Access-Control-Allow-Origin: ¿qué origen está permitido?
- Access-Control-Allow-Credentials: ¿también se aceptan solicitudes cuando el modo de credenciales es incluir (include)?
- Access-Control-Allow-Headers: ¿qué cabeceras pueden utilizarse?
 - Access-Control-Allow-Methods: ¿qué métodos de petición HTTP están permitidos?
- Access-Control-Expose-Headers: ¿qué cabeceras pueden mostrarse?
- Access-Control-Max-Age: ¿cuándo pierde su validez la solicitud preflight?
- Access-Control-Request-Headers: ¿qué header HTTP se indica en la solicitud preflight?
- Access-Control-Request-Method: ¿qué método de petición HTTP se indica en la solicitud preflight?
- Origin: ¿de qué origen proviene la solicitud?
El primer header es especialmente importante, ya que especifica desde qué otro host se puede acceder al servidor solicitado. Además de una dirección concreta, en dicha cabecera también se puede incluir una wildcard en forma de asterisco. De esta manera, el servidor permitirá cross-origin requests de cualquier origen.
# Solución

Hay muchas soluciones para saltar estas “seguridades”, una de ellas es la aca presentada la cual establece un Proxy Server local al que se le envían las solicitudes desde el proyecto indicando cual es su destino final, para que el proxy modifique lo headers de forma tal que el servidor de destino no los rechace.
# Modo de uso

La Aplicación es tipo terminal a la cual alcanza con solo iniciar, aunque podemos especificar algunos valores de personalización de ejecución como ser:

- a: Asigna la IP de escucha de solicitudes, esto es útil si se tiene mas de una placa de red. De forma estándar inicia como LocalHost
-	p: Asigna un puerto de escucha, por defecto es el 19191
-	t: Establece un TimeOut para el seridor de destino, por defecto esta en 5 segundos (Es un poco corto pero en general esta bien)
-	k: Establece un Keep User Agent
Una vez que este en ejecución solo debemos pasar por el proxy antes de mandar la solicitud al servidor destino. 

O sea que si originalmente nuestro código era asi:

```javascript
GetData.open('POST', 'http://' + ip + '/……', true);
```

Ahora deberá ser asi:

```javascript
GetData.open('POST', 'http://localhost:19191?url=http://' + ip + '/……', true);
```

En caso que debamos hacer una autenticación BASIC no podremos usar el típico truco de mandarlo via URL (http://USUARIO:CLAVE@www.....), sino que deberemos insertar esos datos en el propio header asi:

```javascript
GetData.setRequestHeader('Authorization', 'Basic ' + btoa(unescape(encodeURIComponent(USUARIO + ':' + CLAVE))));
```

Eso es todo. Espero que les guste y les sea útil.

# Seguime en:
https://www.linkedin.com/in/fernando-p-maniglia/

# Conocenos más en:
https://www.seamansrl.com.ar
