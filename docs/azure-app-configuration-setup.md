# Azure App Configuration setup

This is the manual Azure portal setup required for PizzaApp. The application uses Microsoft Entra authentication, not an App Configuration connection string, in normal development and deployment.

## 1. Configure the App Configuration store

1. Open the existing **App Configuration** resource in the Azure portal.
2. On **Overview**, copy the **Endpoint**. It is shaped like `https://<store-name>.azconfig.io`.
3. Open **Configuration explorer** and create these key-values:

   | Key | Label | Value |
   | --- | --- | --- |
   | `PizzaApp:Ordering:MaximumToppings` | `Development` | `4` |
   | `PizzaApp:Ordering:MaximumToppings` | `Production` | Your production limit |

   Labels must exactly match the application environment names: `Development` and `Production`. An unlabeled value is optional as a shared fallback.
4. Open **Feature manager** and create the `PizzaManagement` feature flag for the `Development` label. Enable it for initial validation. Create a `Production`-labeled version and choose its release state separately.

The `PizzaManagement` flag controls pizza creation, editing, and deletion. Browsing pizzas remains available when it is disabled.

## 2. Authorize local development

1. In the App Configuration resource, open **Access control (IAM)**.
2. Select **Add > Add role assignment**.
3. Select the **App Configuration Data Reader** role.
4. Assign the role to the Microsoft Entra user you use locally.
5. Authenticate that identity in a terminal with:

   ```sh
   az login
   ```

Role changes can take a few minutes to propagate.

## 3. Configure the local endpoint

Do not place the endpoint or credentials in `appsettings*.json`. From the repository root, place the endpoint in .NET User Secrets:

```sh
dotnet user-secrets init --project PizzaApp
dotnet user-secrets set --project PizzaApp AppConfiguration:Endpoint "https://<store-name>.azconfig.io"
```

Run `dotnet user-secrets list --project PizzaApp` to confirm the endpoint is present. `DefaultAzureCredential` uses the Azure CLI sign-in above or a supported IDE identity locally.

## 4. Configure Azure App Service

1. Open the target **App Service** resource.
2. Open **Identity**, select the **System assigned** tab, switch **Status** to **On**, and save. Copy the principal name or ID if useful for confirming the assignment.
3. Return to the App Configuration resource and add another **App Configuration Data Reader** IAM role assignment for that App Service managed identity.
4. Return to the App Service, open **Settings > Environment variables** (or **Configuration** in the older portal experience), and add an application setting:

   | Name | Value |
   | --- | --- |
   | `AppConfiguration__Endpoint` | `https://<store-name>.azconfig.io` |

5. Save the setting and restart or redeploy the App Service.

The double underscore maps to the .NET configuration path `AppConfiguration:Endpoint`. The managed identity obtains tokens automatically; no client secret or App Configuration connection string is needed.

## Connection-string fallback

Use a connection string only for short-lived troubleshooting when Entra authentication is unavailable. In the App Configuration resource, open **Access keys** (sometimes shown as **Access settings**) and copy the **read-only** connection string. Store it only in .NET User Secrets:

```sh
dotnet user-secrets set --project PizzaApp ConnectionStrings:AppConfiguration "<read-only-connection-string>"
```

Never commit a connection string or production secret. PizzaApp currently uses the endpoint and Entra path only, so the connection-string value has no effect unless a future implementation explicitly adds that fallback.

## Confirm dynamic updates

The provider checks App Configuration at most once every 30 seconds and only when the app receives requests. After changing a setting or flag in the portal, wait for the interval, make a request, then make another request or refresh the browser to observe the updated value. If an update check fails, the app continues using its cached values and retries on later requests.
