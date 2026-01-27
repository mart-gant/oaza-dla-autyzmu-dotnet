# E2E Tests with Playwright

## Setup

### 1. Install Playwright Browsers

Before running E2E tests, you need to install Playwright browsers:

```bash
# Option 1: Using PowerShell script
pwsh tests/OazaDlaAutyzmu.Tests/bin/Debug/net10.0/playwright.ps1 install

# Option 2: Using dotnet tool
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

### 2. Enable Tests

Once browsers are installed, remove the `Skip` attribute from test methods or run all E2E tests:

```bash
dotnet test --filter "FullyQualifiedName~E2E"
```

## Test Categories

### Authentication Tests
- User registration flow
- User login/logout
- Admin dashboard access

### Facility Tests
- Browse facilities list
- Search facilities
- View facility details
- Submit reviews (authenticated users)

### Gallery Tests
- View gallery images
- Lightbox functionality
- Image upload (admin only)

### Contact Tests
- Submit contact form
- Form validation
- Character counter
- Admin message management

## Running Tests

### Run all E2E tests (browsers required)
```bash
dotnet test --filter "Category=E2E"
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~AuthenticationTests"
```

### Run in headed mode (visible browser)
Modify `PlaywrightFixture.cs`:
```csharp
Headless = false  // Change to see browser window
```

## Troubleshooting

### "Playwright browser not found"
Run `playwright install` to download browsers.

### "Port already in use"
Ensure no other instance of the application is running on port 5050.

### Tests timeout
Increase timeout in test methods or check application startup logs.

## CI/CD Integration

For CI/CD pipelines, Playwright browsers can be installed automatically:

```yaml
# GitHub Actions
- name: Install Playwright
  run: |
    dotnet build tests/OazaDlaAutyzmu.Tests
    pwsh tests/OazaDlaAutyzmu.Tests/bin/Debug/net10.0/playwright.ps1 install
```

## Notes

- All E2E tests are skipped by default until Playwright browsers are installed
- Tests use headless Chromium browser by default
- Each test class uses a shared fixture to optimize browser initialization
- Tests run against http://localhost:5050
