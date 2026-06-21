# Rat.Api - Web server API template (my app architecture)

* Onion architecture with clearly separated Core, Infrastructure and API layers
* Simple, stateless REST API system
* Dynamic library scanning that discovers and wires up Rat.* assemblies via reflection
* Support for MsSQL and MySQL databases through pluggable data providers (linq2db)
* Automatic database migration on application startup (creates and alters tables from entity definitions)
* JWT authentication with a cookie-based token and a deny-list for logout
* Language localization support with a configurable default language
* Logging of API events to the database (with a console fallback when the database is unavailable)
* Parametrized, metadata-driven programming for the administration UI
* Experimental common entity service that resolves entities by name at runtime for fast and simple client code

<br>
<br>

# Rat React client application

* Strongly typed with TypeScript
* Separate public web and administration layouts
* Multi-language support
* Common forms and controls with built-in validation
* User and role management
* Table of logged events
* Super-fast administration view definition
* Optimized production build with a bundle report page