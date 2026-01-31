# Sample Application Configuration Instructions

This document explains how to configure environment variables for testing the Autodesk Platform Services SDK samples using a `.env` file.

---

## Required Environment Variables

The following variables are used across the sample classes:

| Variable | Description | Example |
|----------|-------------|---------|
| `TOKEN` | OAuth access token (2-legged or 3-legged) | `eyJhbGciOiJSUzI1NiIsInR5cCI6...` |
| `HUB_ID` | BIM 360/ACC Hub ID | `b.a4f95080-84fe-4281-8d0a-bd8c885695e0` |
| `PROJECT_ID` | Project ID | `b.180e1bc8-6687-4029-a069-319f611de8a9` |
| `FOLDER_ID` | Folder URN | `urn:adsk.wipprod:fs.folder:co.xxxx` |
| `ITEM_ID` | Item URN | `urn:adsk.wipprod:dm.lineage:xxxx` |
| `VERSION_ID` | Version URN | `urn:adsk.wipprod:fs.file:vf.xxxx?version=1` |
| `DOWNLOAD_ID` | Download ID | `download-id-here` |
| `JOB_ID` | Async job ID | `job-id-here` |
| `STORAGE_URN` | Storage object URN | `urn:adsk.objects:os.object:bucket/file.rvt` |
| `BUCKET_KEY` | OSS bucket key | `my-unique-bucket-name` |

---

## Setup Instructions

### Step 1: Create the `.env` File

Create a new text file named `.env` in the `samples` folder (the same directory as `Program.cs`).


### Step 2: Add Your Configuration

Open the `.env` file and paste the following template, then replace the placeholder values with your actual credentials:

    # Autodesk Platform Services - Sample Application Configuration
    # Replace the placeholder values below with sample values.
    # This file type is excluded from git by the .gitignore and should never be committed to source control.

    TOKEN=your_access_token_here
    HUB_ID=b.xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    PROJECT_ID=b.xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    FOLDER_ID=urn:adsk.wipprod:fs.folder:co.xxxxxxxxxxxx
    ITEM_ID=urn:adsk.wipprod:dm.lineage:xxxxxxxxxxxx
    VERSION_ID=urn:adsk.wipprod:fs.file:vf.xxxxxxxxxxxx?version=1
    DOWNLOAD_ID=your_download_id
    JOB_ID=your_job_id
    STORAGE_URN=urn:adsk.objects:os.object:bucket/file.rvt
    BUCKET_KEY=your-bucket-key

### Step 3: Run the Samples

1. Open the solution in Visual Studio
2. Uncomment the sample methods you want to run in `Program.cs`
3. Press **F5** or click **Start** to run the application

The `DotNetEnv.Env.Load()` call in `Program.cs` automatically loads the variables from the `.env` file.
