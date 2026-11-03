# Trazza Platform — REST API Endpoints

This catalog lists every endpoint exposed by `Trazza.Platform`, generated from the controllers. All routes are documented interactively with OpenAPI and Scalar at `/scalar/v1` (Development environment).

## Conventions

- Base path: `/api/v1`, kebab-case resource names, lowercase URLs.
- Content type: `application/json`, camelCase properties, dates as `yyyy-MM-dd`, timestamps in ISO 8601.
- Authentication: `Authorization: Bearer <token>` obtained from `POST /api/v1/authentication/sign-in` or `sign-up`.
- Errors: RFC 9457 `ProblemDetails` with `status`, `title`, `detail`, `instance` and the domain error `code` extension. Messages are localized with `Accept-Language: es` or `en`.
- Enumerated values travel in snake_case (`picked_up`, `in_transit`, `closed_van`, `damaged_goods`).

## IAM

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `POST` | `/api/v1/authentication/sign-in` | Sign in | No | Authenticates a user with email and password and returns the user with a JWT bearer token. |
| `POST` | `/api/v1/authentication/sign-up` | Sign up | No | Registers a carrier or merchant user and returns the user with a JWT bearer token. |
| `GET` | `/api/v1/users/{userId}` | Get user by id | Yes | Gets a user by its unique identifier. |
| `GET` | `/api/v1/users` | Get all users | Yes | Gets all registered users. |
| `PUT` | `/api/v1/users/{userId}` | Update user contact | Yes | Updates the full name and phone of a user and synchronizes the related profile. |

## Profiles

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/carrier-profiles` | Get carrier profiles | Yes | Gets all carrier profiles, optionally filtered by user id. |
| `GET` | `/api/v1/carrier-profiles/{carrierProfileId}` | Get carrier profile by id | Yes | Gets a carrier profile with its vehicles. |
| `POST` | `/api/v1/carrier-profiles` | Create carrier profile | Yes | Creates the carrier profile of a user with carrier role. |
| `PUT` | `/api/v1/carrier-profiles/{carrierProfileId}` | Update carrier profile | Yes | Updates the contact data and synchronizes the vehicle fleet of a carrier profile. |
| `POST` | `/api/v1/carrier-profiles/{carrierProfileId}/vehicles` | Register vehicle | Yes | Registers a vehicle in the fleet of a carrier profile. |
| `PUT` | `/api/v1/carrier-profiles/{carrierProfileId}/vehicles/{vehicleId}` | Update vehicle | Yes | Updates the data or the active state of a vehicle. |
| `DELETE` | `/api/v1/carrier-profiles/{carrierProfileId}/vehicles/{vehicleId}` | Remove vehicle | Yes | Removes a vehicle from the fleet of a carrier profile. |
| `GET` | `/api/v1/merchant-profiles` | Get merchant profiles | Yes | Gets all merchant profiles, optionally filtered by user id. |
| `GET` | `/api/v1/merchant-profiles/{merchantProfileId}` | Get merchant profile by id | Yes | Gets a merchant profile by its unique identifier. |
| `POST` | `/api/v1/merchant-profiles` | Create merchant profile | Yes | Creates the merchant profile of a user with merchant role. |
| `PUT` | `/api/v1/merchant-profiles/{merchantProfileId}` | Update merchant profile | Yes | Updates the business data of a merchant profile. |

## Matchmaking & Routing

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/freight-requests` | Get freight requests | Yes | Gets all freight requests, optionally filtered by merchant user id. |
| `GET` | `/api/v1/freight-requests/{freightRequestId}` | Get freight request by id | Yes | Gets a freight request by its unique identifier. |
| `POST` | `/api/v1/freight-requests` | Create freight request | Yes | Creates a freight request as draft or publishes it when status is open. |
| `PUT` | `/api/v1/freight-requests/{freightRequestId}` | Update freight request | Yes | Edits a draft freight request and applies the requested status transition. |
| `DELETE` | `/api/v1/freight-requests/{freightRequestId}` | Delete freight request | Yes | Deletes a draft or cancelled freight request. |
| `POST` | `/api/v1/freight-requests/{freightRequestId}/publication` | Publish freight request | Yes | Publishes a draft freight request, validating the plan publication limit. |
| `POST` | `/api/v1/freight-requests/{freightRequestId}/cancellation` | Cancel freight request | Yes | Cancels a draft or open freight request and closes its open proposals. |
| `GET` | `/api/v1/freight-requests/{freightRequestId}/carrier-suggestions` | Get carrier suggestions | Yes | Gets the active return routes compatible with the freight request, ordered by detour. |
| `GET` | `/api/v1/match-proposals` | Get match proposals | Yes | Gets all match proposals, optionally filtered by a participant user id. |
| `GET` | `/api/v1/match-proposals/{matchProposalId}` | Get match proposal by id | Yes | Gets a match proposal by its unique identifier. |
| `POST` | `/api/v1/match-proposals` | Send match proposal | Yes | Sends a rate proposal between a return route and a freight request after evaluating compatibility. |
| `PUT` | `/api/v1/match-proposals/{matchProposalId}` | Update match proposal | Yes | Applies the negotiation transition expressed by status, rate and last offer party. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/acceptance` | Accept match proposal | Yes | Accepts the current rate when it is the turn of the given party. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/counteroffers` | Counter match proposal | Yes | Proposes a different rate when it is the turn of the given party. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/rejection` | Reject match proposal | Yes | Rejects an open match proposal. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/confirmation` | Confirm match | Yes | Confirms an accepted proposal, reserves route capacity, closes competitors and opens the shipment. |
| `GET` | `/api/v1/return-routes` | Get return routes | Yes | Gets all return routes, optionally filtered by carrier user id. |
| `GET` | `/api/v1/return-routes/{returnRouteId}` | Get return route by id | Yes | Gets a return route by its unique identifier. |
| `POST` | `/api/v1/return-routes` | Publish return route | Yes | Publishes a return route for one of the carrier vehicles, validating capacity and the plan limit. |
| `PUT` | `/api/v1/return-routes/{returnRouteId}` | Update return route | Yes | Applies a status or capacity change to a return route through its domain rules. |
| `POST` | `/api/v1/return-routes/{returnRouteId}/closure` | Close return route | Yes | Closes a return route and closes its open proposals. |
| `GET` | `/api/v1/return-routes/{returnRouteId}/load-suggestions` | Get load suggestions | Yes | Gets the open freight requests compatible with the return route, ordered by detour. |

## Service Execution & Monitoring

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/shipments` | Get shipments | Yes | Gets all shipments, optionally filtered by a participant user id. |
| `GET` | `/api/v1/shipments/{shipmentId}` | Get shipment by id | Yes | Gets a shipment with its timeline, incidents and live tracking indicators. |
| `POST` | `/api/v1/shipments` | Open shipment | Yes | Opens the shipment of a confirmed match; returns the existing one when it was already opened. |
| `PUT` | `/api/v1/shipments/{shipmentId}` | Update shipment | Yes | Applies the status, location and incident changes of a shipment through its domain rules. |
| `POST` | `/api/v1/shipments/{shipmentId}/pickup` | Confirm pickup | Yes | Confirms that the carrier picked up the cargo. |
| `POST` | `/api/v1/shipments/{shipmentId}/locations` | Share location | Yes | Updates the live location of a moving shipment and detects route deviations. |
| `POST` | `/api/v1/shipments/{shipmentId}/delivery` | Confirm delivery | Yes | Confirms the delivery; requires acknowledgement when the vehicle is far from the address. |
| `POST` | `/api/v1/shipments/{shipmentId}/reception` | Confirm reception | Yes | Confirms the reception by the merchant and closes the shipment. |
| `POST` | `/api/v1/shipments/{shipmentId}/cancellation` | Cancel shipment | Yes | Cancels a shipment that was not picked up yet. |
| `POST` | `/api/v1/shipments/{shipmentId}/incidents` | Report incident | Yes | Reports an incident of the shipment up to 48 hours after delivery. |
| `POST` | `/api/v1/shipments/{shipmentId}/incidents/{incidentId}/resolution` | Resolve incident | Yes | Marks an open incident of the shipment as resolved. |

## Payment & Billing

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/payment-transactions` | Get payment transactions | Yes | Gets all payment transactions, optionally filtered by user id. |
| `GET` | `/api/v1/payment-transactions/{paymentTransactionId}` | Get payment transaction by id | Yes | Gets a payment transaction by its unique identifier. |
| `POST` | `/api/v1/payment-transactions` | Upgrade plan | Yes | Charges the Pro plan to the card through the payment gateway and records the transaction as paid or failed. |
| `GET` | `/api/v1/receipts` | Get receipts | Yes | Gets all electronic receipts, optionally filtered by user id. |
| `GET` | `/api/v1/receipts/{receiptId}` | Get receipt by id | Yes | Gets an electronic receipt by its unique identifier. |
| `POST` | `/api/v1/receipts` | Issue receipt | Yes | Issues a boleta or factura for a paid transaction with the next correlative number. |
| `GET` | `/api/v1/subscription-plans` | Get subscription plans | No | Gets the catalog of subscription plans with price and monthly publication limit. |
| `GET` | `/api/v1/users/{userId}/subscription` | Get user subscription | Yes | Gets the current plan of a user; Free when there is no paid period covering today. |

## Loyalty & Reputation

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/ratings` | Get ratings | Yes | Gets all ratings, optionally filtered by rated user, rater or shipment. |
| `GET` | `/api/v1/ratings/{ratingId}` | Get rating by id | Yes | Gets a rating by its unique identifier. |
| `POST` | `/api/v1/ratings` | Rate counterpart | Yes | Rates the counterpart of a delivered shipment once per rater. |
| `GET` | `/api/v1/users/{userId}/reputation` | Get user reputation | Yes | Gets the average score, total and positive ratings received by a user. |

## Status Codes

| Code | Meaning |
|:--|:--|
| 200 | Successful read or state transition |
| 201 | Resource created (`Location` header points to the new resource) |
| 204 | Resource deleted |
| 400 | Validation error (`EErrorType.Validation`) |
| 401 | Missing or invalid bearer token, or invalid credentials |
| 403 | Forbidden by a business rule (publication limit, only the merchant confirms, not a party) |
| 404 | Resource not found |
| 409 | Conflict with the current state (duplicated email, out of turn, invalid transition) |
| 500 | Unexpected error handled by `GlobalExceptionHandler` |
