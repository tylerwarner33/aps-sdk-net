using Microsoft.VisualStudio.TestTools.UnitTesting;

// These tests change shared cloud state, so they must not run in parallel.
// The attribute is source code, so no configuration file or command line switch can override it.
[assembly: DoNotParallelize]
