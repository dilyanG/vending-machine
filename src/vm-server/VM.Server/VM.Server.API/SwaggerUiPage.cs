namespace VM.Server.API;

public static class SwaggerUiPage
{
    public const string Html = """
        <!DOCTYPE html>
        <html>
        <head>
          <title>VM.Server API</title>
          <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css" />
        </head>
        <body>
          <div id="swagger-ui"></div>
          <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
          <script>
            window.onload = () => {
              window.ui = SwaggerUIBundle({
                url: '/openapi/v1.json',
                dom_id: '#swagger-ui',
              });
            };
          </script>
        </body>
        </html>
        """;
}
