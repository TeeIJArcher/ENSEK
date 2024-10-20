using Controllers;
using Services;
using Models;
using Mocks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit.Sdk;

namespace ENSEK.Tests;

public class ReadingProcessing
{
    private MockCSVService _csvService = new MockCSVService();
    private MockEnsekDbService _ensekDbService = new MockEnsekDbService();

    [Fact]
    public async void TestWithNoCacheAndNoCSVContent()
    {
        // Setup test
        var uploader = new ReadingUploadController(_ensekDbService, _csvService);
        var csvStream = new MemoryStream(new byte[0]);
        var csvFile = new FormFile(csvStream, 0, csvStream.Length, "CSV", "CSV.csv");

        var result = await uploader.Post(new FormFileCollection
        {
            csvFile
        });
        Assert.Equivalent(((OkObjectResult)result).Value as Summary, 
        new Summary {
                totalReadingsUploaded = 0,
                totalUniqueAccounts = 0,
                totalValidAccounts = 0,
                totalValidReadings = 0,
                totalUnknownAccountReadings = 0,
                totalInvalidReadings = 0,
                validValueReadings = new List<Reading>(),
                invalidValueReadings = new List<Reading>(),
                unknownAccountReadings = new List<Reading>()
        });    
    }
    
    [Fact]
    public async void NoCachedReadingsAndFourReadingForTwoAccountsAllValidContent()
    {
        _ensekDbService.AccountCache = new List<AccountDoc> {
            new AccountDoc {
                AccountId = 1,
                FirstName = "Tommy",
                LastName = "Test",
            },
            new AccountDoc {
                AccountId = 2,
                FirstName = "Barry",
                LastName = "Test",
            },
        };
        _csvService.setMockReadResults<ReadingDoc>(new List<ReadingDoc> { 
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 09:24",
                MeterReadValue = 1,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 10:24",
                MeterReadValue = 10,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 08:24",
                MeterReadValue = 1,
                AccountId = 2
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 11:24",
                MeterReadValue = 10,
                AccountId = 2
            }
        });

        // Setup test
        var uploader = new ReadingUploadController(_ensekDbService, _csvService);
        var csvStream = new MemoryStream(new byte[0]);
        var csvFile = new FormFile(csvStream, 0, csvStream.Length, "CSV", "CSV.csv");

        var result = await uploader.Post(new FormFileCollection
        {
            csvFile
        });
        var actual = ((OkObjectResult)result).Value as Summary;
        var expect = new Summary {
                totalReadingsUploaded = 4,
                totalUniqueAccounts = 2,
                totalValidAccounts = 2,
                totalValidReadings = 4,
                totalUnknownAccountReadings = 0,
                totalInvalidReadings = 0,
                validValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 09:24",
                        MeterReadValue = 1,
                        AccountId = 1
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 10:24",
                        MeterReadValue = 10,
                        AccountId = 1
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 08:24",
                        MeterReadValue = 1,
                        AccountId = 2
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 11:24",
                        MeterReadValue = 10,
                        AccountId = 2
                    }
                },
                invalidValueReadings = new List<Reading>(),
                unknownAccountReadings = new List<Reading>()
        };

        Assert.NotNull(actual);
        Assert.Equal(actual.totalReadingsUploaded, expect.totalReadingsUploaded);
        Assert.Equal(actual.totalUniqueAccounts, expect.totalUniqueAccounts);
        Assert.Equal(actual.totalValidAccounts, expect.totalValidAccounts);
        Assert.Equal(actual.totalValidReadings, expect.totalValidReadings);
        Assert.Equal(actual.totalUnknownAccountReadings, expect.totalUnknownAccountReadings);
        Assert.Equal(actual.totalInvalidReadings, expect.totalInvalidReadings);
    }
    
    [Fact]
    public async void NoCachedReadingsAndFourReadingForTwoAccountsTwoValidContent()
    {
        _ensekDbService.AccountCache = new List<AccountDoc> {
            new AccountDoc {
                AccountId = 1,
                FirstName = "Tommy",
                LastName = "Test",
            },
            new AccountDoc {
                AccountId = 2,
                FirstName = "Barry",
                LastName = "Test",
            },
        };
        _csvService.setMockReadResults<ReadingDoc>(new List<ReadingDoc> { 
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 09:24",
                MeterReadValue = 10,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 10:24",
                MeterReadValue = 1,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 08:24",
                MeterReadValue = 10,
                AccountId = 2
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 11:24",
                MeterReadValue = 1,
                AccountId = 2
            }
        });

        // Setup test
        var uploader = new ReadingUploadController(_ensekDbService, _csvService);
        var csvStream = new MemoryStream(new byte[0]);
        var csvFile = new FormFile(csvStream, 0, csvStream.Length, "CSV", "CSV.csv");

        var result = await uploader.Post(new FormFileCollection
        {
            csvFile
        });
        var actual = ((OkObjectResult)result).Value as Summary;
        var expect = new Summary {
                totalReadingsUploaded = 4,
                totalUniqueAccounts = 2,
                totalValidAccounts = 2,
                totalValidReadings = 2,
                totalUnknownAccountReadings = 0,
                totalInvalidReadings = 2,
                validValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 09:24",
                        MeterReadValue = 10,
                        AccountId = 1
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 08:24",
                        MeterReadValue = 10,
                        AccountId = 2
                    },
                },
                invalidValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 10:24",
                        MeterReadValue = 1,
                        AccountId = 1
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 11:24",
                        MeterReadValue = 1,
                        AccountId = 2
                    }
                },
                unknownAccountReadings = new List<Reading>()
        };

        Assert.NotNull(actual);
        Assert.Equal(actual.totalReadingsUploaded, expect.totalReadingsUploaded);
        Assert.Equal(actual.totalUniqueAccounts, expect.totalUniqueAccounts);
        Assert.Equal(actual.totalValidAccounts, expect.totalValidAccounts);
        Assert.Equal(actual.totalValidReadings, expect.totalValidReadings);
        Assert.Equal(actual.totalUnknownAccountReadings, expect.totalUnknownAccountReadings);
        Assert.Equal(actual.totalInvalidReadings, expect.totalInvalidReadings);
    }
    
    
    [Fact]
    public async void NoCachedReadingsAndFourReadingForTwoAccountsOneValidContentTwoUnknown()
    {
        _ensekDbService.AccountCache = new List<AccountDoc> {
            new AccountDoc {
                AccountId = 1,
                FirstName = "Tommy",
                LastName = "Test",
            }
        };
        _csvService.setMockReadResults<ReadingDoc>(new List<ReadingDoc> { 
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 09:24",
                MeterReadValue = 10,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 10:24",
                MeterReadValue = 1,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 08:24",
                MeterReadValue = 10,
                AccountId = 2
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 11:24",
                MeterReadValue = 1,
                AccountId = 2
            }
        });

        // Setup test
        var uploader = new ReadingUploadController(_ensekDbService, _csvService);
        var csvStream = new MemoryStream(new byte[0]);
        var csvFile = new FormFile(csvStream, 0, csvStream.Length, "CSV", "CSV.csv");

        var result = await uploader.Post(new FormFileCollection
        {
            csvFile
        });
        var actual = ((OkObjectResult)result).Value as Summary;
        var expect = new Summary {
                totalReadingsUploaded = 4,
                totalUniqueAccounts = 2,
                totalValidAccounts = 1,
                totalValidReadings = 1,
                totalUnknownAccountReadings = 2,
                totalInvalidReadings = 1,
                validValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 09:24",
                        MeterReadValue = 10,
                        AccountId = 1
                    }
                },
                invalidValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 10:24",
                        MeterReadValue = 1,
                        AccountId = 1
                    }
                },
                unknownAccountReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 11:24",
                        MeterReadValue = 1,
                        AccountId = 2
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 08:24",
                        MeterReadValue = 10,
                        AccountId = 2
                    },
                }
        };

        Assert.NotNull(actual);
        Assert.Equal(actual.totalReadingsUploaded, expect.totalReadingsUploaded);
        Assert.Equal(actual.totalUniqueAccounts, expect.totalUniqueAccounts);
        Assert.Equal(actual.totalValidAccounts, expect.totalValidAccounts);
        Assert.Equal(actual.totalValidReadings, expect.totalValidReadings);
        Assert.Equal(actual.totalUnknownAccountReadings, expect.totalUnknownAccountReadings);
        Assert.Equal(actual.totalInvalidReadings, expect.totalInvalidReadings);
    }

    
    [Fact]
    public async void NoCachedReadingsAndFourReadingForTwoAccountsNoValidContentTwoUnknown()
    {
        _ensekDbService.AccountCache = new List<AccountDoc> {
            new AccountDoc {
                AccountId = 1,
                FirstName = "Tommy",
                LastName = "Test",
            }
        };
        _ensekDbService.ReadingCache = new List<ReadingDoc> {
            new ReadingDoc {
                AccountId = 1,
                MeterReadingDateTime = "22/04/2019 11:24",
                MeterReadValue = 9999,
            }
        };
        _csvService.setMockReadResults<ReadingDoc>(new List<ReadingDoc> { 
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 09:24",
                MeterReadValue = 10,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 10:24",
                MeterReadValue = 1,
                AccountId = 1
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 08:24",
                MeterReadValue = 10,
                AccountId = 2
            },
            new ReadingDoc {
                MeterReadingDateTime = "22/04/2019 11:24",
                MeterReadValue = 1,
                AccountId = 2
            }
        });

        // Setup test
        var uploader = new ReadingUploadController(_ensekDbService, _csvService);
        var csvStream = new MemoryStream(new byte[0]);
        var csvFile = new FormFile(csvStream, 0, csvStream.Length, "CSV", "CSV.csv");

        var result = await uploader.Post(new FormFileCollection
        {
            csvFile
        });
        var actual = ((OkObjectResult)result).Value as Summary;
        var expect = new Summary {
                totalReadingsUploaded = 4,
                totalUniqueAccounts = 2,
                totalValidAccounts = 1,
                totalValidReadings = 0,
                totalUnknownAccountReadings = 2,
                totalInvalidReadings = 2,
                validValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 09:24",
                        MeterReadValue = 10,
                        AccountId = 1
                    }
                },
                invalidValueReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 10:24",
                        MeterReadValue = 1,
                        AccountId = 1
                    }
                },
                unknownAccountReadings = new List<Reading> {
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 11:24",
                        MeterReadValue = 1,
                        AccountId = 2
                    },
                    new Reading {
                        MeterReadingDateTime = "22/04/2019 08:24",
                        MeterReadValue = 10,
                        AccountId = 2
                    },
                }
        };

        Assert.NotNull(actual);
        Assert.Equal(actual.totalReadingsUploaded, expect.totalReadingsUploaded);
        Assert.Equal(actual.totalUniqueAccounts, expect.totalUniqueAccounts);
        Assert.Equal(actual.totalValidAccounts, expect.totalValidAccounts);
        Assert.Equal(actual.totalValidReadings, expect.totalValidReadings);
        Assert.Equal(actual.totalUnknownAccountReadings, expect.totalUnknownAccountReadings);
        Assert.Equal(actual.totalInvalidReadings, expect.totalInvalidReadings);
    }
}