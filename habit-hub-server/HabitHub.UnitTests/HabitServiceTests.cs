using HabitHub.API.Exceptions;
using HabitHub.API.Models.DTOs;
using HabitHub.API.Services;
using HabitHub.Data;
using HabitHub.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HabitHub.UnitTests
{
    public class HabitServiceTests
    {
        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }


        private async Task<(Guid creatorId, Guid teamId, Guid habitId)>
            SeedTeamAndHabitAsync(
                AppDbContext db,
                HabitType habitType = HabitType.Binary,
                HabitState habitState = HabitState.Active)
        {
            var creator = new TeamCreator
            {
                Id = Guid.NewGuid(),
                Name = "Creator",
                Email = $"{Guid.NewGuid()}@test.com",
                Timezone = "UTC",
                PasswordHash = "hash"
            };
            db.Users.Add(creator);

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                CreatorId = creator.Id
            };
            db.Teams.Add(team);

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Test Habit",
                Goal = "Be consistent",
                Type = habitType,
                State = habitState,
                TeamId = team.Id,
                Unit = "times"
            };
            db.Habits.Add(habit);

            await db.SaveChangesAsync();
            return (creator.Id, team.Id, habit.Id);
        }



        private async Task<Guid> SeedMemberAsync(AppDbContext db, Guid teamId)
        {
            var member = new TeamMember
            {
                Id = Guid.NewGuid(),
                Name = "Member",
                Email = $"{Guid.NewGuid()}@test.com",
                Timezone = "UTC",
                PasswordHash = "hash"
            };
            db.Users.Add(member);

            db.Memberships.Add(new Membership
            {
                TeamId = teamId,
                UserId = member.Id,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return member.Id;
        }



        private HabitEntry MakeEntry(Guid habitId, Guid userId, DateTime date,
    EntryStatus status = EntryStatus.Logged, float? value = null)
        {
            return new HabitEntry
            {
                HabitId = habitId,
                UserId = userId,
                Date = date,
                Status = status,
                Value = value,
                Notes = ""
            };
        }

        //---------------------------- UpdateHabitAsync

        [Fact]
        public async Task UpdateHabit_ShouldThrowNotFound_WhenHabitDoesNotExist()
        {
            var db = CreateDb();
            var service = new HabitService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateHabitAsync(Guid.NewGuid(), new UpdateHabitDto { Name = "X" }, Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateHabit_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var service = new HabitService(db);

            // Member tries to update — only creator is allowed
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.UpdateHabitAsync(habitId, new UpdateHabitDto { Name = "New Name" }, memberId));
        }

        [Fact]
        public async Task UpdateHabit_ShouldThrowValidation_WhenHabitTypeIsInvalid()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.UpdateHabitAsync(habitId, new UpdateHabitDto { HabitType = "InvalidType" }, creatorId));
        }

        [Fact]
        public async Task UpdateHabit_ShouldThrowValidation_WhenExpiryDateIsInThePast()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.UpdateHabitAsync(habitId, new UpdateHabitDto
                {
                    ExpiryDate = DateTime.UtcNow.AddDays(-1) // yesterday
                }, creatorId));
        }

        [Fact]
        public async Task UpdateHabit_ShouldUpdateFields_WhenCreatorUpdates()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            var result = await service.UpdateHabitAsync(habitId, new UpdateHabitDto
            {
                Name = "Updated Name",
                Goal = "Updated Goal",
                HabitType = "Quantitative",
                Unit = "km",
                ExpiryDate = DateTime.UtcNow.AddDays(30)
            }, creatorId);

            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Goal", result.Goal);
            Assert.Equal("Quantitative", result.HabitType);
            Assert.Equal("km", result.Unit);
            Assert.NotNull(result.ExpiryDate);
        }

        [Fact]
        public async Task UpdateHabit_ShouldOnlyUpdateProvidedFields_WhenSomeFieldsAreNull()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            // Only update Name, leave everything else untouched
            var result = await service.UpdateHabitAsync(habitId, new UpdateHabitDto
            {
                Name = "Only Name Changed"
            }, creatorId);

            Assert.Equal("Only Name Changed", result.Name);
            Assert.Equal("Be consistent", result.Goal); // unchanged from seed
        }


        // ---------------------------------ArchiveHabitAsync

        [Fact]
        public async Task ArchiveHabit_ShouldThrowNotFound_WhenHabitDoesNotExist()
        {
            var db = CreateDb();
            var service = new HabitService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.ArchiveHabitAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ArchiveHabit_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var (_, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.ArchiveHabitAsync(habitId, memberId));
        }

        [Fact]
        public async Task ArchiveHabit_ShouldThrowConflict_WhenHabitIsAlreadyClosed()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitState: HabitState.Closed);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.ArchiveHabitAsync(habitId, creatorId));
        }

        [Fact]
        public async Task ArchiveHabit_ShouldReturnSilently_WhenHabitIsAlreadyArchived()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitState: HabitState.Archived);
            var service = new HabitService(db);

            await service.ArchiveHabitAsync(habitId, creatorId);

            var habit = await db.Habits.FindAsync(habitId);
            Assert.Equal(HabitState.Archived, habit.State);
        }

        [Fact]
        public async Task ArchiveHabit_ShouldSetStateToArchived_WhenHabitIsActive()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            await service.ArchiveHabitAsync(habitId, creatorId);

            var habit = await db.Habits.FindAsync(habitId);
            Assert.Equal(HabitState.Archived, habit.State);
        }

        // ------------------------------------DeleteHabitAsync


        [Fact]
        public async Task DeleteHabit_ShouldThrowNotFound_WhenHabitDoesNotExist()
        {
            var db = CreateDb();
            var service = new HabitService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteHabitAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteHabit_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var (_, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.DeleteHabitAsync(habitId, memberId));
        }

        [Fact]
        public async Task DeleteHabit_ShouldRemoveHabitAndEntries_WhenCreatorDeletes()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);

            // Seed an entry so we can verify it gets deleted too
            db.HabitEntries.Add(MakeEntry(habitId, creatorId, DateTime.UtcNow.Date));
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            await service.DeleteHabitAsync(habitId, creatorId);

            Assert.Null(await db.Habits.FindAsync(habitId));
            Assert.Empty(db.HabitEntries.Where(e => e.HabitId == habitId));
        }


        // -----------------------------------LogProgressAsync


        [Fact]
        public async Task LogProgress_ShouldThrowNotFound_WhenHabitDoesNotExist()
        {
            var db = CreateDb();
            var service = new HabitService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.LogProgressAsync(Guid.NewGuid(), Guid.NewGuid(), new LogProgressDto { Status = "Logged" }));
        }

        [Fact]
        public async Task LogProgress_ShouldThrowForbidden_WhenUserIsNotMemberOrCreator()
        {
            var db = CreateDb();
            var (_, _, habitId) = await SeedTeamAndHabitAsync(db);
            var outsider = Guid.NewGuid(); // not seeded into the team at all
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.LogProgressAsync(habitId, outsider, new LogProgressDto { Status = "Logged" }));
        }

        [Fact]
        public async Task LogProgress_ShouldThrowConflict_WhenHabitIsNotActive()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitState: HabitState.Archived);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.LogProgressAsync(habitId, creatorId, new LogProgressDto { Status = "Logged" }));
        }

        [Fact]
        public async Task LogProgress_ShouldThrowConflict_WhenAlreadyLoggedToday()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);

            // Pre-seed today's entry
            db.HabitEntries.Add(MakeEntry(habitId, creatorId, DateTime.UtcNow.Date));
            await db.SaveChangesAsync();

            var service = new HabitService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.LogProgressAsync(habitId, creatorId, new LogProgressDto { Status = "Logged" }));
        }

        [Fact]
        public async Task LogProgress_ShouldThrowValidation_WhenStatusIsInvalid()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.LogProgressAsync(habitId, creatorId, new LogProgressDto { Status = "InvalidStatus" }));
        }

        [Fact]
        public async Task LogProgress_ShouldThrowValidation_WhenQuantitativeHabitHasNoValue()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Quantitative);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.LogProgressAsync(habitId, creatorId, new LogProgressDto
                {
                    Status = "Logged",
                    Value = null  // missing for quantitative
                }));
        }

        [Fact]
        public async Task LogProgress_ShouldThrowValidation_WhenBinaryHabitHasValue()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Binary);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.LogProgressAsync(habitId, creatorId, new LogProgressDto
                {
                    Status = "Logged",
                    Value = 5.0f  // should not be provided for binary
                }));
        }

        [Fact]
        public async Task LogProgress_ShouldCreateEntry_WhenBinaryHabitLoggedSuccessfully()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Binary);
            var service = new HabitService(db);

            var result = await service.LogProgressAsync(habitId, creatorId, new LogProgressDto
            {
                Status = "Logged",
                Notes = "Felt great"
            });

            Assert.Equal(habitId, result.HabitId);
            Assert.Equal(creatorId, result.UserId);
            Assert.Equal("Logged", result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Felt great", result.Notes);
        }

        [Fact]
        public async Task LogProgress_ShouldCreateEntry_WhenQuantitativeHabitLoggedSuccessfully()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Quantitative);
            var service = new HabitService(db);

            var result = await service.LogProgressAsync(habitId, creatorId, new LogProgressDto
            {
                Status = "Logged",
                Notes = "",
                Value = 7.5f
            });

            Assert.Equal(7.5f, result.Value);
            Assert.Equal("Logged", result.Status);
        }

        [Fact]
        public async Task LogProgress_ShouldCreateEntry_WhenStatusIsSkipped()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Binary);
            var service = new HabitService(db);

            var result = await service.LogProgressAsync(habitId, creatorId, new LogProgressDto
            {
                Status = "Skipped",
                Notes = ""
            });

            Assert.Equal("Skipped", result.Status);
        }

        [Fact]
        public async Task LogProgress_ShouldAlsoWork_WhenCallerIsActiveMember()
        {
            var db = CreateDb();
            var (_, teamId, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Binary);
            var memberId = await SeedMemberAsync(db, teamId);
            var service = new HabitService(db);

            // Members should be able to log their own progress
            var result = await service.LogProgressAsync(habitId, memberId, new LogProgressDto
            {
                Status = "Logged",
                Notes = ""
            });

            Assert.Equal(memberId, result.UserId);
        }

        //--------------------------------------------- UndoLogAsync


        [Fact]
        public async Task UndoLog_ShouldThrowConflict_WhenHabitIsNotActive()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db, habitState: HabitState.Archived);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.UndoLogAsync(habitId, Guid.NewGuid(), creatorId));
        }

        [Fact]
        public async Task UndoLog_ShouldThrowNotFound_WhenEntryDoesNotExist()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);
            var service = new HabitService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UndoLogAsync(habitId, Guid.NewGuid(), creatorId));
        }

        [Fact]
        public async Task UndoLog_ShouldRemoveEntry_WhenEntryExists()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);

            var entry = new HabitEntry
            {
                Id = Guid.NewGuid(),
                HabitId = habitId,
                UserId = creatorId,
                Date = DateTime.UtcNow.Date,
                Notes = "",
                Status = EntryStatus.Logged
            };
            db.HabitEntries.Add(entry);
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            await service.UndoLogAsync(habitId, entry.Id, creatorId);

            Assert.Null(await db.HabitEntries.FindAsync(entry.Id));
        }

        // -----------------------------------------------GetProgressAsync

        [Fact]
        public async Task GetProgress_ShouldReturnOwnEntries_WhenNoMemberIdProvided()
        {
            var db = CreateDb();
            var (creatorId, _, habitId) = await SeedTeamAndHabitAsync(db);

            db.HabitEntries.Add(MakeEntry(habitId, creatorId, DateTime.UtcNow.Date));
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            var result = await service.GetProgressAsync(habitId, creatorId, memberId: null);

            Assert.Single(result);
            Assert.Equal(creatorId, result[0].UserId);
        }

        [Fact]
        public async Task GetProgress_ShouldThrowForbidden_WhenNonCreatorViewsMemberProgress()
        {
            var db = CreateDb();
            var (_, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);
            var anotherMemberId = await SeedMemberAsync(db, teamId);
            var service = new HabitService(db);

            // memberId trying to view anotherMemberId's progress — not allowed
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.GetProgressAsync(habitId, memberId, memberId: anotherMemberId));
        }

        [Fact]
        public async Task GetProgress_ShouldReturnMemberEntries_WhenCreatorViewsMemberProgress()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db);
            var memberId = await SeedMemberAsync(db, teamId);

            db.HabitEntries.Add(new HabitEntry
            {
                Id = Guid.NewGuid(),
                HabitId = habitId,
                UserId = memberId,
                Date = DateTime.UtcNow.Date,
                Notes = "",
                Status = EntryStatus.Logged
            });
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            var result = await service.GetProgressAsync(habitId, creatorId, memberId: memberId);

            Assert.Single(result);
            Assert.Equal(memberId, result[0].UserId);
        }

        // ---------------------------------------------------GetLeaderboardAsync

        [Fact]
        public async Task GetLeaderboard_ShouldCountDays_ForBinaryHabit()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Binary);
            var memberId = await SeedMemberAsync(db, teamId);

            // Creator logged 1 day, member logged 2 days
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = creatorId, Date = DateTime.UtcNow.Date, Notes = "", Status = EntryStatus.Logged });
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = memberId, Date = DateTime.UtcNow.Date, Notes = "", Status = EntryStatus.Logged });
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = memberId, Date = DateTime.UtcNow.Date.AddDays(-1), Notes = "", Status = EntryStatus.Logged });
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            var result = await service.GetLeaderboardAsync(habitId, creatorId);

            // Member should be first with 2 days
            Assert.Equal(memberId, result[0].UserId);
            Assert.Equal(2f, result[0].Progress);
        }

        [Fact]
        public async Task GetLeaderboard_ShouldSumValues_ForQuantitativeHabit()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Quantitative);
            var memberId = await SeedMemberAsync(db, teamId);

            db.Memberships.Add(new Membership
            {
                TeamId = teamId,
                UserId = creatorId,
                Status = MembershipStatus.Active
            });

            // Creator logged 10, member logged 3+4=7
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = creatorId, Date = DateTime.UtcNow.Date, Value = 10f, Notes = "", Status = EntryStatus.Logged });
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = memberId, Date = DateTime.UtcNow.Date, Value = 3f, Notes = "", Status = EntryStatus.Logged });
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = memberId, Date = DateTime.UtcNow.Date.AddDays(-1), Notes = "", Value = 4f, Status = EntryStatus.Logged });
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            var result = await service.GetLeaderboardAsync(habitId, creatorId);

            // Creator should be first with 10
            Assert.Equal(creatorId, result[0].UserId);
            Assert.Equal(10f, result[0].Progress);
            Assert.Equal(7f, result[1].Progress);
        }

        [Fact]
        public async Task GetLeaderboard_ShouldNotCountSkippedEntries()
        {
            var db = CreateDb();
            var (creatorId, teamId, habitId) = await SeedTeamAndHabitAsync(db, habitType: HabitType.Binary);
            var memberId = await SeedMemberAsync(db, teamId);

            // Member has 1 logged + 1 skipped — only logged should count
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = memberId, Date = DateTime.UtcNow.Date, Notes = "", Status = EntryStatus.Logged });
            db.HabitEntries.Add(new HabitEntry { Id = Guid.NewGuid(), HabitId = habitId, UserId = memberId, Date = DateTime.UtcNow.Date.AddDays(-1), Notes = "", Status = EntryStatus.Skipped });
            await db.SaveChangesAsync();

            var service = new HabitService(db);
            var result = await service.GetLeaderboardAsync(habitId, creatorId);

            var memberEntry = result.First(r => r.UserId == memberId);
            Assert.Equal(1f, memberEntry.Progress);
        }
    }
}