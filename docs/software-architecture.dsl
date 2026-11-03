workspace "Trazza Platform" "C4 model of the Trazza logistics SaaS platform." {

    model {
        carrier = person "Carrier" "Transportista with empty return trips who publishes routes and executes shipments."
        merchant = person "Merchant" "Comerciante that publishes freight requests and confirms matches."

        trazza = softwareSystem "Trazza" "Connects carriers with empty return trips and merchants who need to ship goods in Lima." {
            landingPage = container "Landing Page" "Public marketing site." "HTML, CSS, JavaScript"
            webApp = container "Web Application" "Single page application for carriers and merchants." "Vue 3, PrimeVue, Vite"
            api = container "Trazza Platform API" "RESTful Web Services organized in DDD bounded contexts." "ASP.NET Core, .NET 10, C# 14" {
                iam = component "IAM" "Users, sign-up, sign-in, JWT and request authorization." "Bounded Context"
                profiles = component "Profiles" "Carrier profiles with vehicles and merchant profiles." "Bounded Context"
                matchmaking = component "Matchmaking & Routing" "Return routes, freight requests, proposals and RouteMatchingService." "Bounded Context"
                execution = component "Service Execution & Monitoring" "Shipments, live tracking, delivery and incidents." "Bounded Context"
                billing = component "Payment & Billing" "Subscription plans, payment transactions and receipts." "Bounded Context"
                reputation = component "Loyalty & Reputation" "Ratings and reputation summaries." "Bounded Context"
                shared = component "Shared Kernel" "Money, Address, GeoLocation, Result, Error, interceptors and pipeline behaviors." "Bounded Context"
            }
            database = container "Database" "Stores users, profiles, routes, requests, proposals, shipments, payments and ratings." "MySQL" "Database"
        }

        paymentGateway = softwareSystem "Payment Gateway" "Card processor (simulated in this version)." "External"

        carrier -> landingPage "Visits"
        merchant -> landingPage "Visits"
        carrier -> webApp "Publishes routes, negotiates and executes trips"
        merchant -> webApp "Publishes requests, negotiates and tracks shipments"
        landingPage -> webApp "Redirects to sign-in and sign-up"
        webApp -> api "Calls" "JSON/HTTPS, Bearer JWT"
        api -> database "Reads and writes" "EF Core, MySQL"
        api -> paymentGateway "Charges subscription payments" "HTTPS"

        webApp -> iam "Authenticates"
        profiles -> iam "Queries users through IIamContextFacade"
        matchmaking -> profiles "Queries vehicles through IProfilesContextFacade"
        matchmaking -> billing "Checks publication limit through IBillingContextFacade"
        execution -> matchmaking "Handles MatchProposalConfirmedEvent"
        execution -> profiles "Queries contacts through IProfilesContextFacade"
        reputation -> execution "Queries shipment parties through IExecutionContextFacade"
        billing -> iam "Queries users through IIamContextFacade"
        billing -> paymentGateway "Charges through IPaymentGateway"
        iam -> database "Persists users"
        profiles -> database "Persists profiles and vehicles"
        matchmaking -> database "Persists routes, requests and proposals"
        execution -> database "Persists shipments, events and incidents"
        billing -> database "Persists transactions and receipts"
        reputation -> database "Persists ratings"
    }

    views {
        systemContext trazza "SystemContext" {
            include *
            autolayout lr
        }

        container trazza "Containers" {
            include *
            autolayout lr
        }

        component api "Components" {
            include *
            autolayout lr
        }

        styles {
            element "Person" {
                shape person
                background #0b3d91
                color #ffffff
            }
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            element "Container" {
                background #438dd5
                color #ffffff
            }
            element "Component" {
                background #85bbf0
                color #000000
            }
            element "Database" {
                shape cylinder
            }
            element "External" {
                background #999999
                color #ffffff
            }
        }
    }
}
