# CI secrets and variables

Every test in this repository calls the live Autodesk Platform Services API.
There is no unit test tier, because `SDKManager` builds its own `ApsClient`, which builds its own
`ForgeService`. No test can intercept HTTP traffic today.

So the workflows need real credentials. This file records what to create and where.

**A missing value gives an inconclusive test until the environment is configured, then a failure.**
`ApsTestConfig.RequireString` names the value it wanted.

- On a developer machine, and in CI before the environment holds any client credentials, it throws
  `AssertInconclusiveException`. The cause is missing configuration, not a defect in the code under test.
- In CI, once `APS_CLIENT_ID` or `APS_SSA_CLIENT_ID` is set, any other missing value **fails** the
  test. A partly configured environment is a configuration defect, and an inconclusive result there
  would give a green build that did not run its tests.
- A token request that APS rejects always **fails** the test. The credentials were present, so an
  expired or revoked secret must turn the build red.

---

## Where these live

Create a GitHub **Environment** named `aps-integration-tests`, not repository-level secrets.

An environment can require a reviewer, and it is not readable from a fork pull request.
That second property is what protects these values on a public repository.

Settings > Environments > New environment > `aps-integration-tests`.

**A fork pull request therefore builds but runs no tests.** That is a GitHub security boundary, not
a setting, and it is the reason the replay lane matters. See the follow-up section at the end.

---

## 1. Secrets, 9 of them

These are sensitive. Store them as environment **secrets**.

| # | Name | Purpose | Used by | How to obtain |
| --- | --- | --- | --- | --- |
| 1 | `APS_CLIENT_ID` | two-legged client id | oss, modelderivative, webhooks, secureserviceaccount, authentication | an APS app on <https://aps.autodesk.com/myapps> |
| 2 | `APS_CLIENT_SECRET` | two-legged client secret | same as above | the same APS app |
| 3 | `APS_SSA_CLIENT_ID` | client id of the app that owns the service account | datamanagement, accountadmin, issues | a confidential APS app that owns the service account |
| 4 | `APS_SSA_CLIENT_SECRET` | secret of that app | same as above | the same app |
| 5 | `APS_SSA_SERVICE_ACCOUNT_ID` | the `sub` claim of the JWT assertion | same as above | the `CreateServiceAccountAsync` response, field `serviceAccountId` |
| 6 | `APS_SSA_KEY_ID` | the `kid` header of the JWT assertion | same as above | the `CreateServiceAccountKeyAsync` response, field `kid` |
| 7 | `APS_SSA_PRIVATE_KEY` | PEM RSA private key that signs the assertion | same as above | the `CreateServiceAccountKeyAsync` response. Returned **once only**. Paste the full PEM including the header and footer lines. |
| 8 | `APS_WEBHOOK_CALLBACK_URL` | callback endpoint for created hooks | webhooks | a reachable HTTPS endpoint you control. A request bin is acceptable. |
| 9 | `APS_WEBHOOK_SECRET_TOKEN` | value used by the create and update token tests | webhooks | any string of 16 to 64 characters that you choose |

---

## 2. Variables, 17 of them

These are not sensitive. Store them as environment **variables**.

| # | Name | Purpose | Used by | How to obtain |
| --- | --- | --- | --- | --- |
| 10 | `APS_REGION` | region header, default `US` | all | one of `US`, `EMEA`, `AUS`, `CAN`, `DEU`, `IND`, `JPN`, `GBR` |
| 11 | `OSS_BUCKET_PREFIX` | readable prefix for generated bucket keys, default `apssdk-ci` | oss | choose a short lowercase prefix |
| 12 | `APS_TEST_BUCKET_SALT` | hashed into every derived resource name: bucket keys, issue titles, and service account names | oss, issues, secureserviceaccount | generate once, ex. a random GUID. See "Why the salt is a variable" below. |
| 13 | `WEBHOOKS_WORKFLOW_ID` | the `scope.workflow` value on a created hook | webhooks | any stable string you choose |
| 14 | `MD_SOURCE_URN` | base64 URN of an **untranslated** object, for `StartJobAsync` | modelderivative | upload a model to OSS, then base64-encode the object URN |
| 15 | `MD_TRANSLATED_URN` | base64 URN of a model **already translated** to SVF2 | modelderivative | translate once by hand, then confirm the manifest reports `complete` |
| 16 | `MD_RUN_TRANSLATION_TEST` | set to `false` to switch off the one billed test | modelderivative | optional, defaults to `true` |
| 17 | `DM_HUB_ID` | hub for the read tests | datamanagement | `GetHubsAsync` |
| 18 | `DM_PROJECT_ID` | project inside that hub | datamanagement | `GetHubProjectsAsync` |
| 19 | `DM_FOLDER_ID` | folder that holds at least one item | datamanagement | `GetProjectTopFoldersAsync` |
| 20 | `DM_ITEM_ID` | item inside that folder | datamanagement | `GetFolderContentsAsync` |
| 21 | `DM_VERSION_ID` | a version of that item | datamanagement | `GetItemVersionsAsync` |
| 22 | `ACC_ACCOUNT_ID` | ACC or BIM 360 account | accountadmin | ACC Account Admin, or `GetHubsAsync` with the `b.` prefix removed |
| 23 | `ACC_PROJECT_ID` | project in that account | accountadmin | `GetProjectsAsync` |
| 24 | `ACC_USER_ID` | a user who is a member of that project | accountadmin | the ACC Account Admin members list |
| 25 | `ISSUES_PROJECT_ID` | ACC project with the Issues module active | issues | the same as `ACC_PROJECT_ID` when Issues is active there |
| 26 | `ISSUES_ISSUE_SUBTYPE_ID` | required by the `CreateIssueAsync` payload | issues | `GetIssuesTypesAsync`, then read a subtype id |
| 27 | `ISSUES_ASSIGNEE_USER_ID` | required by the `CreateIssueAsync` payload | issues | `GetUserProfileAsync`, field `id` |

### Optional variables

| Name | Purpose |
| --- | --- |
| `DM_DOWNLOAD_ID` | only if the removed `GetDownloadAsync` test is restored |
| `DM_JOB_ID` | only if the removed `GetDownloadJobAsync` test is restored |

### Three-legged values, which CI deliberately does not set

Three Authentication tests exercise the three-legged flow. That flow needs a browser redirect and a
human, so it cannot be automated. **Leave these unset in CI.** The tests then report inconclusive
with a message saying why, which is the honest result: the code was not exercised, and it did not
fail either.

| Name | Used by | How to obtain |
| --- | --- | --- |
| `APS_THREE_LEGGED_CODE` | `GetThreeLeggedTokenAsync_...` | the `code` query parameter from a completed authorization redirect. Expires in minutes. |
| `APS_THREE_LEGGED_REFRESH_TOKEN` | `RefreshTokenAsync_...` | the `refresh_token` from a three-legged token response |
| `APS_THREE_LEGGED_ACCESS_TOKEN` | `GetUserInfoAsync_...` | the `access_token` from a three-legged token response |
| `APS_THREE_LEGGED_REDIRECT_URI` | `GetThreeLeggedTokenAsync_...` | the callback URL registered on the APS app. Defaults to `https://localhost/callback`. |

Set them locally in `.env` when you want to exercise that path by hand.

The equivalent server-to-server path **is** covered automatically, by the service account assertion
exchange in `ApsTestTokens.GetServiceAccountAsync`.

---

## 3. Why the salt is a variable and not a secret

OSS bucket keys share **one global namespace across every APS tenant**, and the service accepts only
lowercase letters, digits, `-`, `_`, and `.`.

A naive key such as `apssdk-ci-{clientId}` would publish the client id into a namespace that anyone
can probe, simply by attempting to create the same key. So every generated name is a SHA-256 digest
of the test name plus a tenant salt. A digest is one way, so the key reveals nothing.

The salt is a **variable** rather than a secret for one reason: the future replay lane runs with no
credentials at all, so it cannot derive a client-id hash. It must be able to read the salt and
compute the same key the recording holds. The salt is a random GUID tied to no credential, so
publishing it costs nothing.

`ApsTestNaming` is the single definition. Do not derive a resource name anywhere else.

---

## 4. Local development

Put the same names in a `.env` file at or above the repository root.
`ApsTestConfig` loads it on first read, through `DotNetEnv`.
A value that is already set in the process environment wins over the file.
In CI the file is never read, so a stray `.env` cannot shadow a GitHub secret.

The existing `*.env` pattern in `.gitignore` already matches a bare `.env`, because a gitignore `*`
matches an empty sequence. No new ignore entry is needed, and no credential can reach a commit
through that path.

On a developer machine the derived resource names carry a `dev-<hash of machine name>` scope instead
of `ci`, so a local run cannot collide with a CI run or with another developer.

---

## 5. What the tests leave behind

Read this before enabling the live lane against an account that matters.

| Resource | Residue | Why |
| --- | --- | --- |
| OSS buckets | **one shared fixture bucket, forever** | The fixture key is derived and constant, so every run reuses the same bucket and never deletes it. OSS can keep a deleted key unavailable for some time, so a fixture deleted at the end of one run could be missing at the start of the next. The create and delete bucket tests use a key that also carries the GitHub run id, and delete that bucket in a `finally` block. |
| OSS objects | none, if cleanup runs | Every object test deletes its object in a `finally` block. All buckets use `PolicyKey.Transient`, so an object that an aborted run leaves expires in 24 hours. |
| ACC issues | one closed issue per issue-creating test per run | **An ACC issue cannot be deleted through the API.** Cleanup can only close it. Every created issue is titled with a `CI` prefix so a project owner can recognise it. |
| Service accounts | none, if cleanup runs | An application may hold at most 10 accounts and 3 keys per account. Cleanup runs in `[TestCleanup]`, per test, not only at the end of the class. Names carry the GitHub run id, so an account that a killed run leaves cannot block the next run, but it still counts against the quota. The list operation does not return the name, so there is no safe automatic sweep. Delete such an account by hand. |
| Webhooks | none, if cleanup runs | Every hook is a Model Derivative `extraction.finished` hook scoped by `WEBHOOKS_WORKFLOW_ID`, with a `ci-hook` query value on `APS_WEBHOOK_CALLBACK_URL`. The class deletes every such hook before it starts, so a killed run leaves nothing for long. |
| Webhooks secret token | none, if cleanup runs | The token tests set and remove the application-wide secret token. See the security section. |
| Model Derivative | none | The one translation job produces a derivative on an existing URN. |

---

## 6. Security

Read this before you create the environment.

**Use APS applications that exist only for CI.**
The tests change application-wide state: they set and delete the Webhooks secret token for every hook
of the application, they create and delete service accounts and keys, and they delete OSS buckets
they own.
Never point them at an application that serves anything else.

**Grant the least scope.**
Each suite requests only the scopes it needs, and each workflow passes only the secrets its suite
reads. Keep it that way when you add a test.

**Use a callback URL that only CI uses.**
The Webhooks suite deletes every hook on `APS_WEBHOOK_CALLBACK_URL` that carries its `ci-hook` marker.

**Who can run code with these secrets.**
A push to `main`, a manual dispatch, and a pull request from a branch of this repository run with the
secrets. A fork pull request does not, and that is a GitHub boundary, not a setting. Only people with
write access can push a branch to this repository, so write access is the trust boundary. Keep it
small, and review a change to `.github/` or `test/Autodesk.Aps.TestCommon/` as carefully as a change
to a secret.

**Test results are public artifacts.**
On a public repository, anyone can download the uploaded test results for 14 days.
No test writes a secret to its output, and the token helper reports only the documented error fields
of a failed token request, never the response body. Keep it that way.

**Every action is pinned to a full commit SHA.**
`.github/dependabot.yml` proposes updates weekly, with a seven day cooldown so that a compromised
release has time to be found before it is proposed.

## 7. Cost

Everything here is free except one call.

`StartJobAsync_WithSvf2AndThumbnailOutputs_ReturnsCreatedResult` starts a translation job, and
translation **consumes cloud credits**. Every other Model Derivative test reads an already
translated URN or reads static data, and those are free.

Set `MD_RUN_TRANSLATION_TEST` to `false` to switch that one test off without a code change.

Use the smallest valid source model for `MD_SOURCE_URN`. Credit cost scales with model complexity.

GitHub Actions minutes, artifact storage, environments, and the merge queue are all free here,
because both repositories are public and every workflow uses `ubuntu-latest`.
**Larger runners are billed even on a public repository.** Never change `runs-on` without a
deliberate decision.

---

## 8. Pull request review checklist

Check these before approving any pull request that touches the test tree.

- No token, key, id, or URL is hardcoded in a `.cs` file.
- No new `Environment.GetEnvironmentVariable` call. Everything goes through `ApsTestConfig`.
- No resource name is derived anywhere but `ApsTestNaming`.
- No `.runsettings` file gains a `TestRunParameters` or `EnvironmentVariables` element.
- If recordings are ever added, no `authenticate.json` carries a real `access_token`. Autodesk's own
  public Design Automation repository committed a JWT containing a live client id this way. A token
  expires; a client id does not.

---

## 9. What still needs a maintainer

Three parts of this design need write or admin access on the upstream repository, so an outside
contributor cannot do them.

| Task | Why it needs a maintainer |
| --- | --- |
| Creating the `aps-integration-tests` environment and everything in this file | Environment configuration is a repository setting |
| Enabling the merge queue, with maximum entries to build set to 1 | Branch protection is an admin setting |
| Adding the aggregate gate to required status checks | Same |

A required check that listens only for `pull_request` will sit as "expected" forever and block every
merge once a merge queue is enabled. Any workflow used as a required check must also trigger on
`merge_group`.

---

## 10. Follow-up: a test lane that needs no secrets

Everything above exists because there is no way to intercept HTTP traffic in these SDKs.

`Autodesk.SDKManager` 1.2.0 adds that seam: `SdkManagerBuilder.Add(IApsClient)`, and an `ApsClient`
constructor that accepts a prebuilt `ForgeService`. A test can then build a `ForgeService` over its
own `HttpMessageHandler`.

The service packages reference the **published** `Autodesk.SDKManager` package through
`Directory.Packages.props`, not the `sdkmanager/` project. So the tests can use the seam only after
1.2.0 is published and that pin moves from 1.1.2.

That unlocks record and replay, which the Autodesk Design Automation SDK already uses through
`Autodesk.Forge.Core.E2eTestHelpers`. Its CI runs `dotnet test` with **zero** test secrets.

The shape to aim for:

| Lane | Question it answers | Trigger | Secrets | Speed |
| --- | --- | --- | --- | --- |
| Replay | Did this code change break the SDK's behavior? | every push and pull request | none | seconds |
| Live | Did the APS service change under us? | merge queue, release, manual dispatch | yes | minutes |

Replay is the only lane an external contributor can ever run, so it is worth more than it first
looks on a public SDK.
