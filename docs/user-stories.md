# Trazza Platform — REST API Technical Stories

## Overview

This document contains API-focused technical stories intended for the frontend developers integrating `trazza-webapp` with the Trazza Platform REST API (ASP.NET Core / .NET 10). Each technical story references the user stories of the web application (`US-xxx`).

Common conventions:
- Base path: `/api/v1`
- Content-Type: `application/json`
- Error responses follow RFC 9457 `ProblemDetails` (`type`, `title`, `status`, `detail`, `instance`, `code`).
- Authentication: `Authorization: Bearer <token>`.

## Traceability

| Technical Story | Bounded Context | User Stories | Endpoints |
|:--|:--|:--|:--|
| TS-IAM-001 Sign Up | IAM | US-001, US-002 | `POST /authentication/sign-up` |
| TS-IAM-002 Sign In | IAM | US-003, US-005 | `POST /authentication/sign-in` |
| TS-IAM-003 Update User Contact | IAM / Profiles | US-006 | `PUT /users/{userId}` |
| TS-PRO-001 Create Carrier Profile | Profiles | US-001 | `POST /carrier-profiles` |
| TS-PRO-002 Create Merchant Profile | Profiles | US-002 | `POST /merchant-profiles` |
| TS-PRO-003 Manage Vehicles | Profiles | US-007, US-008 | `POST/PUT/DELETE /carrier-profiles/{id}/vehicles`, `PUT /carrier-profiles/{id}` |
| TS-MAT-001 Publish Return Route | Matchmaking | US-009, US-029 | `POST /return-routes` |
| TS-MAT-002 Close Return Route | Matchmaking | US-010 | `POST /return-routes/{id}/closure`, `PUT /return-routes/{id}` |
| TS-MAT-003 Load Suggestions | Matchmaking | US-011 | `GET /return-routes/{id}/load-suggestions` |
| TS-MAT-004 Create Freight Request | Matchmaking | US-014, US-029 | `POST /freight-requests` |
| TS-MAT-005 Manage Freight Requests | Matchmaking | US-015 | `PUT/DELETE /freight-requests/{id}`, `/publication`, `/cancellation` |
| TS-MAT-006 Carrier Suggestions | Matchmaking | US-016 | `GET /freight-requests/{id}/carrier-suggestions` |
| TS-MAT-007 Send Proposal | Matchmaking | US-012, US-013, US-016 | `POST /match-proposals` |
| TS-MAT-008 Negotiate Proposal | Matchmaking | US-017 | `/acceptance`, `/counteroffers`, `/rejection`, `PUT /match-proposals/{id}` |
| TS-MAT-009 Confirm Match | Matchmaking / Execution | US-018 | `POST /match-proposals/{id}/confirmation` |
| TS-EXE-001 Confirm Pickup | Execution | US-019 | `POST /shipments/{id}/pickup` |
| TS-EXE-002 Share Location | Execution | US-020, US-022 | `POST /shipments/{id}/locations` |
| TS-EXE-003 Confirm Delivery and Reception | Execution | US-021, US-023 | `/delivery`, `/reception` |
| TS-EXE-004 Report Incident | Execution | US-024 | `POST /shipments/{id}/incidents` |
| TS-EXE-005 Shipment History | Execution | US-022, US-025 | `GET /shipments?userId=` |
| TS-BIL-001 Upgrade to Pro | Billing | US-026, US-027 | `POST /payment-transactions`, `GET /users/{id}/subscription` |
| TS-BIL-002 Issue Receipt | Billing | US-028 | `POST /receipts` |
| TS-REP-001 Rate Counterpart | Reputation | US-030 | `POST /ratings` |
| TS-REP-002 View Reputation | Reputation | US-031 | `GET /ratings?targetId=`, `GET /users/{id}/reputation` |

---

## Bounded Context: IAM

### TS-IAM-001 — Sign Up
As a frontend developer, I want to register carriers and merchants through the API so that the sign-up form creates the account and opens a session.

- **Scenario: Successful sign-up**
  - Given a `POST /api/v1/authentication/sign-up` with `fullName`, `email`, `phone`, `role` (`carrier` | `merchant`) and `password`
  - When the data is valid and the email is not registered
  - Then the API responds `201 Created` with `id`, `fullName`, `email`, `phone`, `role`, `createdAt` and `token`
- **Scenario: Profile created by domain event**
  - Given the body also includes `documentNumber` (DNI for carriers, RUC for merchants) and `businessName` for merchants
  - Then `UserRegisteredEvent` creates the carrier or merchant profile after the commit
- **Scenario: Email already registered**
  - Then the API responds `409 Conflict` with code `Iam.EmailAlreadyRegistered`
- **Scenario: Invalid data**
  - Given a name shorter than 3 characters, an invalid email, a phone that is not `9XXXXXXXX`, an unknown role or a password shorter than 8 characters
  - Then the API responds `400 Bad Request`

### TS-IAM-002 — Sign In
- **Scenario: Valid credentials**
  - Given a `POST /api/v1/authentication/sign-in` with `email` and `password`
  - Then the API responds `200 OK` with the user and the bearer `token`
- **Scenario: Wrong credentials**
  - Then the API responds `401 Unauthorized` with code `Iam.InvalidCredentials`
- **Scenario: Calling a protected endpoint without token**
  - Then the API responds `401 Unauthorized`

### TS-IAM-003 — Update User Contact
- **Scenario: Valid update**
  - Given a `PUT /api/v1/users/{userId}` with `fullName` and `phone`
  - Then the API responds `200 OK` with the updated user and `UserContactUpdatedEvent` synchronizes the profile
- **Scenario: Invalid phone**
  - Then the API responds `400 Bad Request` with code `Iam.InvalidPhone`

---

## Bounded Context: Profiles

### TS-PRO-001 — Create Carrier Profile
- **Scenario: Successful creation**
  - Given a `POST /api/v1/carrier-profiles` with `userId`, `fullName`, `dni` and `phone` for a user with role `carrier`
  - Then the API responds `201 Created` with the profile and an empty `vehicles` list
- **Scenario: Duplicated DNI or profile**
  - Then the API responds `409 Conflict` (`Profiles.DniAlreadyRegistered`, `Profiles.ProfileAlreadyExists`)
- **Scenario: Role mismatch**
  - Given the user has role `merchant`
  - Then the API responds `400 Bad Request` with code `Profiles.UserRoleMismatch`

### TS-PRO-002 — Create Merchant Profile
- **Scenario: Successful creation**
  - Given a `POST /api/v1/merchant-profiles` with `userId`, `businessName`, `contactName`, `ruc` and `phone`
  - Then the API responds `201 Created` with `isCompany` derived from the RUC prefix
- **Scenario: Invalid RUC**
  - Then the API responds `400 Bad Request` with code `Profiles.InvalidRuc`

### TS-PRO-003 — Manage Vehicles
- **Scenario: Register vehicle**
  - Given a `POST /api/v1/carrier-profiles/{id}/vehicles` with `plate`, `brandModel`, `bodyType`, `capacityKg` and `volumeM3`
  - Then the plate is normalized to `ABC-123` and the API responds `201 Created` with the profile
- **Scenario: Duplicated plate**
  - Then the API responds `409 Conflict` with code `Profiles.VehiclePlateAlreadyRegistered`
- **Scenario: Synchronize fleet**
  - Given a `PUT /api/v1/carrier-profiles/{id}` with `fullName`, `phone` and the full `vehicles` list
  - Then vehicles with a known id are updated, new ones are added and missing ones are removed

---

## Bounded Context: Matchmaking & Routing

### TS-MAT-001 — Publish Return Route
- **Scenario: Valid route**
  - Given a `POST /api/v1/return-routes` with `carrierId`, `vehicleId`, `origin`, `destination`, `departureDate`, `timeWindow`, `availableWeightKg`, `availableVolumeM3`, `maxDetourKm` and `acceptedCargoTypes`
  - Then the API validates the vehicle through `IProfilesContextFacade`, estimates distance and duration with `RouteMatchingService` and responds `201 Created` with status `active`
- **Scenario: Capacity exceeds vehicle**
  - Then the API responds `400 Bad Request` with code `Matchmaking.CapacityExceedsVehicle`
- **Scenario: Past date**
  - Then the API responds `400 Bad Request` with code `Matchmaking.DateInPast`
- **Scenario: Publication limit of the Free plan**
  - Given the user already published 5 routes or requests this month on the Free plan
  - Then the API responds `403 Forbidden` with code `Matchmaking.PublicationLimitReached`

### TS-MAT-002 — Close Return Route
- **Scenario: Closing an active route**
  - Given a `POST /api/v1/return-routes/{id}/closure`
  - Then the route becomes `closed` and `ReturnRouteClosedEvent` closes its open proposals
- **Scenario: Already closed**
  - Then the API responds `409 Conflict` with code `Matchmaking.RouteAlreadyClosed`

### TS-MAT-003 — Load Suggestions
- Given a `GET /api/v1/return-routes/{id}/load-suggestions`
- Then the API responds `200 OK` with the open freight requests compatible with the route and their `detour`, ordered by detour distance

### TS-MAT-004 — Create Freight Request
- **Scenario: Publish**
  - Given a `POST /api/v1/freight-requests` with `status: "open"`
  - Then the request is created as `open` after checking the publication limit
- **Scenario: Draft**
  - Given `status: "draft"`
  - Then the request is created as `draft` without consuming the publication limit
- **Scenario: Same pickup and delivery**
  - Then the API responds `400 Bad Request` with code `Matchmaking.SamePickupDelivery`

### TS-MAT-005 — Manage Freight Requests
- `PUT /freight-requests/{id}` edits drafts and applies the requested transition (`open`, `cancelled`, `matched`).
- `POST /freight-requests/{id}/publication` publishes a draft; `POST /freight-requests/{id}/cancellation` cancels and closes open proposals.
- `DELETE /freight-requests/{id}` deletes drafts or cancelled requests, otherwise `409 Conflict`.

### TS-MAT-006 — Carrier Suggestions
- Given a `GET /api/v1/freight-requests/{id}/carrier-suggestions`
- Then the API responds with the active compatible return routes and their detour.

### TS-MAT-007 — Send Proposal
- **Scenario: Compatible proposal**
  - Given a `POST /api/v1/match-proposals` with `freightRequestId`, `returnRouteId`, `currentRate` and `initiatedBy`
  - Then the API evaluates compatibility and responds `201 Created`; when the carrier proposes the published rate the status is `accepted`, otherwise `pending`
- **Scenario: Duplicated proposal**
  - Then the API responds `409 Conflict` with code `Matchmaking.ProposalAlreadyExists`
- **Scenario: Incompatible**
  - Then the API responds `400 Bad Request` with code `Matchmaking.IncompatibleMatch` and the reasons in `detail`

### TS-MAT-008 — Negotiate Proposal
- **Scenario: Answer in turn**
  - Given `POST /match-proposals/{id}/acceptance` or `/counteroffers` with the `party` whose turn it is
  - Then the API responds `200 OK` with status `accepted` or `counteroffer`
- **Scenario: Out of turn**
  - Then the API responds `409 Conflict` with code `Matchmaking.NotYourTurn`
- **Scenario: Same rate**
  - Then the API responds `400 Bad Request` with code `Matchmaking.CounterofferSameRate`

### TS-MAT-009 — Confirm Match
- **Scenario: Accepted proposal**
  - Given a `POST /api/v1/match-proposals/{id}/confirmation` with `party: "merchant"`
  - Then the proposal becomes `matched`, the request `matched`, the route reserves capacity, competing proposals are closed and `MatchProposalConfirmedEvent` opens the shipment
- **Scenario: Not accepted**
  - Then the API responds `409 Conflict` with code `Matchmaking.ProposalNotAccepted`
- **Scenario: Carrier tries to confirm**
  - Then the API responds `403 Forbidden` with code `Matchmaking.OnlyMerchantConfirms`

---

## Bounded Context: Service Execution & Monitoring

### TS-EXE-001 — Confirm Pickup
- Given `POST /api/v1/shipments/{id}/pickup` for a `matched` shipment
- Then the status becomes `picked_up`, the current location is the pickup district and a `picked_up` event is recorded.

### TS-EXE-002 — Share Location
- Given `POST /api/v1/shipments/{id}/locations` with `latitude` and `longitude`
- Then the status becomes `in_transit`, `deviationKm`, `distanceToDeliveryKm` and `etaMinutes` are recalculated and a `detour_alert` event is recorded when the deviation exceeds 2 km.

### TS-EXE-003 — Confirm Delivery and Reception
- **Scenario: Near the address**
  - Given `POST /shipments/{id}/delivery` within 1 km of the delivery address
  - Then the status becomes `delivered`
- **Scenario: Far from the address**
  - Given `acknowledgeDistance: false` and more than 1 km away
  - Then the API responds `409 Conflict` with code `Execution.FarFromDelivery`
- **Scenario: Reception**
  - Given `POST /shipments/{id}/reception` for a delivered shipment
  - Then the status becomes `closed`

### TS-EXE-004 — Report Incident
- **Scenario: Valid incident**
  - Given `POST /shipments/{id}/incidents` with `type`, `description` (10 to 500 characters), `reporterId` and `reporterRole`
  - Then the API responds `201 Created` with the shipment including the incident
- **Scenario: After 48 hours**
  - Then the API responds `409 Conflict` with code `Execution.IncidentWindowExpired`

### TS-EXE-005 — Shipment History
- Given `GET /api/v1/shipments?userId={id}`
- Then the API returns the shipments where the user is carrier or merchant with their timeline and incidents.

---

## Bounded Context: Payment & Billing

### TS-BIL-001 — Upgrade to Pro
- **Scenario: Approved card**
  - Given a `POST /api/v1/payment-transactions` with `userId`, `plan: "pro"` and `method` (`brand`, `last4`, `holderName`, `expiry`, or `cardNumber`)
  - Then the gateway approves the charge and the API responds `201 Created` with status `paid`, `authorizationCode`, `periodStart` and `periodEnd`
- **Scenario: Declined card**
  - Given a card ending in `0002`
  - Then the transaction is recorded with status `failed`
- **Scenario: Plan already active**
  - Then the API responds `409 Conflict` with code `Billing.PlanAlreadyActive`
- **Scenario: Current plan**
  - Given `GET /api/v1/users/{userId}/subscription`
  - Then the API returns `pro` while a paid period covers today, otherwise `free` with 5 monthly publications

### TS-BIL-002 — Issue Receipt
- **Scenario: Boleta or factura**
  - Given a `POST /api/v1/receipts` with `transactionId`, `type`, `customerName` and `customerDocument`
  - Then the API assigns the next correlative number (`B001-00000001`, `F001-00000001`) and returns `total`, `subtotal` and `igv` (18 %)
- **Scenario: Wrong document**
  - Then the API responds `400 Bad Request` (`Billing.InvalidCustomerDni` or `Billing.InvalidCustomerRuc`)
- **Scenario: Unpaid transaction**
  - Then the API responds `409 Conflict` with code `Billing.TransactionNotPaid`

---

## Bounded Context: Loyalty & Reputation

### TS-REP-001 — Rate Counterpart
- **Scenario: Valid rating**
  - Given a `POST /api/v1/ratings` with `shipmentId`, `raterId`, `score` (1–5), `tags` and `comment`
  - Then the API derives the rated party from the shipment through `IExecutionContextFacade` and responds `201 Created`
- **Scenario: Already rated**
  - Then the API responds `409 Conflict` with code `Reputation.AlreadyRated`
- **Scenario: Tag not allowed**
  - Then the API responds `400 Bad Request` with code `Reputation.InvalidTag`

### TS-REP-002 — View Reputation
- Given `GET /api/v1/ratings?targetId={userId}` or `GET /api/v1/users/{userId}/reputation`
- Then the API returns the received ratings or the summary with `averageScore`, `totalRatings` and `positiveRatings`.
