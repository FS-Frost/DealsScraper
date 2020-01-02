# DealsScraper

Nintendo Switch USA eshop deals scraper using Selenium and ChromeDriver.

# How to use (out of the box)

-   Restore the dependencies.
-   Compile the solution.
-   Edit ConsoleApp\config.json if necessary.
-   Run ConsoleApp.exe.

# How to use with remote driver

-   Run ChromeDriver on any server.
-   On ConsoleApp\config.json, set:
    -   "remoteDriverIp" to the server IP, including the port.
    -   "useRemoteDriver" to "true".
-   Run ConsoleApp.exe.

# How to use with local driver

-   Install ChromeDriver.
-   On ConsoleApp\config.json, set:
    -   "useRemoteDriver" to "false".
    -   "localDriverLocation" to the driver directory ("null" to use executable directory).
-   Run ConsoleApp.exe.
