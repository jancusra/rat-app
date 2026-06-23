> **Note:** This is a demo showcase of my code. Some details are not polished to
> full functional perfection, but the project works as a whole. It is mainly aimed
> at testing generic (common) entities in C# and exploring how far they can be
> pushed in terms of complexity, with the goal of keeping the frontend as simple
> as possible.

<br>

# Quick start

Clone the repository and open its folder:

```
git clone https://github.com/jancusra/rat-app
cd rat-app
```

Then spin up the whole stack (database, API and React client) with a single command:

```
docker compose up
```

Once the containers are up, the app is available at:

* **Web client:** http://localhost:3000 (or `http://<LAN-IP>:3000` from another device)
* **API:** served under the same origin at `/api` (nginx proxies it to the backend), so only port 3000 needs to be reachable.

A default administrator user is seeded automatically:

* **E-mail:** `jra@gmail.com`
* **Password:** `test123`

Stop the stack (keeps the database data):

```
docker compose down
```

Stop the stack and wipe everything, including the database volume:

```
docker compose down -v
```

<br>

<br>

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
* Single-page application with client-side routing (React Router)
* Lazy-loaded, code-split routes for a fast initial load
* Separate public web and administration layouts
* Material UI components with data-grid based entity tables
* Multi-language support
* Cookie-based JWT authentication with login and registration
* Common forms and controls with built-in validation
* Metadata-driven grids and forms generated from server entity definitions
* User and role management
* Table of logged events
* Super-fast administration view definition
* Extensible plugin system for custom pages and routes
* Graceful error handling with a route-aware error boundary
* Accessible, keyboard-navigable UI
* Optimized production build with a bundle report page