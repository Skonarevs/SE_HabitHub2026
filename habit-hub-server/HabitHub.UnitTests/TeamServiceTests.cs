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
    public class TeamServiceTests
    {

        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        /// <summary>
        /// Seeds a TeamCreator. Every test that needs a creator calls this.
        /// </summary>
        private async Task<Guid> SeedCreatorAsync(AppDbContext db)
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
            await db.SaveChangesAsync();
            return creator.Id;
        }

        /// <summary>
        /// Seeds a TeamMember.
        /// </summary>
        private async Task<Guid> SeedMemberAsync(AppDbContext db)
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
            await db.SaveChangesAsync();
            return member.Id;
        }

        /// <summary>
        /// Seeds a Team owned by the given creator, with an optional TeamChat
        /// (required by DeleteTeam). Also adds an active Membership for the creator.
        /// </summary>
        private async Task<Guid> SeedTeamAsync(AppDbContext db, Guid creatorId)
        {
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "Test Team",
                CreatorId = creatorId,
                Chat = new TeamChat(),
                //CreatedAt = DateTime.UtcNow
            };
            db.Teams.Add(team);

            db.Memberships.Add(new Membership
            {
                UserId = creatorId,
                TeamId = team.Id,
                Status = MembershipStatus.Active
            });

            await db.SaveChangesAsync();
            return team.Id;
        }

        /// <summary>
        /// Seeds an active Membership for an existing user in an existing team.
        /// </summary>
        private async Task SeedMembershipAsync(AppDbContext db, Guid teamId, Guid userId)
        {
            db.Memberships.Add(new Membership
            {
                UserId = userId,
                TeamId = teamId,
                Status = MembershipStatus.Active
            });
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds an active InviteCode for a team.
        /// </summary>
        private async Task<string> SeedInviteCodeAsync(AppDbContext db, Guid teamId)
        {
            var code = new InviteCode
            {
                Id = Guid.NewGuid(),
                Code = "TESTCODE",
                TeamId = teamId,
                ExpiryDate = DateTime.UtcNow.AddDays(10),
                State = InviteCodeState.Active
            };
            db.InviteCodes.Add(code);
            await db.SaveChangesAsync();
            return code.Code;
        }




        // CreateTeamAsync -----------------------------------------------------------------------

        [Fact]
        public async Task CreateTeam_ShouldThrowForbidden_WhenUserIsNotACreator()
        {
            var db = CreateDb();
            // Seed a TeamMember (not a TeamCreator)
            var memberId = await SeedMemberAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.CreateTeamAsync(memberId, "My Team"));
        }

        [Fact]
        public async Task CreateTeam_ShouldThrowValidation_WhenTeamNameIsEmpty()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateTeamAsync(creatorId, ""));
        }

        [Fact]
        public async Task CreateTeam_ShouldThrowValidation_WhenTeamNameIsWhitespace()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateTeamAsync(creatorId, "   "));
        }

        [Fact]
        public async Task CreateTeam_ShouldReturnTeam_WhenCreatorCreatesTeam()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var service = new TeamService(db);

            var result = await service.CreateTeamAsync(creatorId, "My Team");

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("My Team", result.Name);
            Assert.Equal(creatorId, result.CreatorId);
        }

        [Fact]
        public async Task CreateTeam_ShouldAlsoCreateMembership_ForCreator()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var service = new TeamService(db);

            var result = await service.CreateTeamAsync(creatorId, "My Team");

            // Creator should automatically become an active member
            var membership = await db.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == result.Id && m.UserId == creatorId);
            Assert.NotNull(membership);
            Assert.Equal(MembershipStatus.Active, membership.Status);
        }



        // CreateHabitAsync -----------------------------------------------------------------------


        [Fact]
        public async Task CreateHabit_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateHabitAsync(Guid.NewGuid(), new CreateHabitDto
                {
                    Name = "Habit",
                    Goal = "Goal",
                    HabitType = "Binary",
                    Unit = "times"
                }, Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateHabit_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.CreateHabitAsync(teamId, new CreateHabitDto
                {
                    Name = "Habit",
                    Goal = "Goal",
                    HabitType = "Binary",
                    Unit = "times"
                }, memberId));
        }

        [Fact]
        public async Task CreateHabit_ShouldThrowValidation_WhenHabitTypeIsInvalid()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateHabitAsync(teamId, new CreateHabitDto
                {
                    Name = "Habit",
                    Goal = "Goal",
                    HabitType = "Invalid",
                    Unit = "times"
                }, creatorId));
        }

        [Fact]
        public async Task CreateHabit_ShouldThrowValidation_WhenQuantitativeHabitHasNoUnit()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateHabitAsync(teamId, new CreateHabitDto
                {
                    Name = "Habit",
                    Goal = "Goal",
                    HabitType = "Quantitative",
                    Unit = null  // missing for quantitative
                }, creatorId));
        }

        [Fact]
        public async Task CreateHabit_ShouldThrowValidation_WhenExpiryDateIsInThePast()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateHabitAsync(teamId, new CreateHabitDto
                {
                    Name = "Habit",
                    Goal = "Goal",
                    HabitType = "Binary",
                    Unit = "times",
                    ExpiryDate = DateTime.UtcNow.AddDays(-1)
                }, creatorId));
        }

        [Fact]
        public async Task CreateHabit_ShouldReturnHabit_WhenBinaryHabitCreated()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            var result = await service.CreateHabitAsync(teamId, new CreateHabitDto
            {
                Name = "Run",
                Goal = "Daily run",
                HabitType = "Binary",
                Unit = "times"
            }, creatorId);

            Assert.Equal("Run", result.Name);
            Assert.Equal("Daily run", result.Goal);
            Assert.Equal("Binary", result.HabitType);
            Assert.Equal("Active", result.State);
            Assert.Equal(teamId, result.TeamId);
        }

        [Fact]
        public async Task CreateHabit_ShouldReturnHabit_WhenQuantitativeHabitCreated()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            var result = await service.CreateHabitAsync(teamId, new CreateHabitDto
            {
                Name = "Run",
                Goal = "Run every day",
                HabitType = "Quantitative",
                Unit = "km"
            }, creatorId);

            Assert.Equal("Quantitative", result.HabitType);
            Assert.Equal("km", result.Unit);
        }


        // GetTeamAsync -----------------------------------------------------------------------



        [Fact]
        public async Task GetTeam_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetTeamAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetTeam_ShouldThrowForbidden_WhenUserIsNotMember()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var outsiderId = Guid.NewGuid();
            var service = new TeamService(db);

            // Note: GetTeamAsync uses !isCreator || !isActiveMember — both must be true
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.GetTeamAsync(teamId, outsiderId));
        }

        [Fact]
        public async Task GetTeam_ShouldReturnTeam_WhenCreatorRequests()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            var result = await service.GetTeamAsync(teamId, creatorId);

            Assert.Equal(teamId, result.Id);
            Assert.Equal("Test Team", result.Name);
        }



        // GenerateInviteCodeAsync -----------------------------------------------------------------------



        [Fact]
        public async Task GenerateInviteCode_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GenerateInviteCodeAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GenerateInviteCode_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.GenerateInviteCodeAsync(teamId, memberId));
        }

        [Fact]
        public async Task GenerateInviteCode_ShouldReturnCode_WhenCreatorGenerates()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            var result = await service.GenerateInviteCodeAsync(teamId, creatorId);

            Assert.NotNull(result.Code);
            Assert.Equal(8, result.Code.Length);
            Assert.True(result.ExpiryDate > DateTime.UtcNow);
        }



        // JoinTeamAsync -----------------------------------------------------------------------


        [Fact]
        public async Task JoinTeam_ShouldThrowNotFound_WhenInviteCodeDoesNotExist()
        {
            var db = CreateDb();
            var memberId = await SeedMemberAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.JoinTeamAsync(memberId, "BADCODE"));
        }

        [Fact]
        public async Task JoinTeam_ShouldThrowConflict_WhenUserIsAlreadyMember()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            await SeedMembershipAsync(db, teamId, memberId);
            var code = await SeedInviteCodeAsync(db, teamId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.JoinTeamAsync(memberId, code));
        }

        [Fact]
        public async Task JoinTeam_ShouldReturnMembership_WhenValidCodeUsed()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            var code = await SeedInviteCodeAsync(db, teamId);
            var service = new TeamService(db);

            var result = await service.JoinTeamAsync(memberId, code);

            Assert.Equal(teamId, result.TeamId);
            Assert.Equal("Active", result.Status);
        }

        [Fact]
        public async Task JoinTeam_ShouldCreateMembership_InDatabase()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            var code = await SeedInviteCodeAsync(db, teamId);
            var service = new TeamService(db);

            await service.JoinTeamAsync(memberId, code);

            var membership = await db.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == memberId);
            Assert.NotNull(membership);
            Assert.Equal(MembershipStatus.Active, membership.Status);
        }


        // KickMemberAsync -----------------------------------------------------------------------


        [Fact]
        public async Task KickMember_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.KickMemberAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task KickMember_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            await SeedMembershipAsync(db, teamId, memberId);
            var service = new TeamService(db);

            // Member trying to kick someone
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.KickMemberAsync(teamId, memberId, memberId));
        }

        [Fact]
        public async Task KickMember_ShouldThrowConflict_WhenCreatorTriesToKickThemself()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.KickMemberAsync(teamId, creatorId, creatorId));
        }

        [Fact]
        public async Task KickMember_ShouldThrowNotFound_WhenMemberIsNotInTeam()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.KickMemberAsync(teamId, Guid.NewGuid(), creatorId));
        }

        [Fact]
        public async Task KickMember_ShouldSetStatusToKicked_WhenSuccessful()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            await SeedMembershipAsync(db, teamId, memberId);
            var service = new TeamService(db);

            await service.KickMemberAsync(teamId, memberId, creatorId);

            var membership = await db.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == memberId);
            Assert.Equal(MembershipStatus.Kicked, membership.Status);
            Assert.NotNull(membership.LeftAt);
        }


        // LeaveTeamAsync -----------------------------------------------------------------------



        [Fact]
        public async Task LeaveTeam_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.LeaveTeamAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task LeaveTeam_ShouldThrowConflict_WhenCreatorTriesToLeave()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.LeaveTeamAsync(teamId, creatorId));
        }

        [Fact]
        public async Task LeaveTeam_ShouldThrowNotFound_WhenUserIsNotMember()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.LeaveTeamAsync(teamId, Guid.NewGuid()));
        }

        [Fact]
        public async Task LeaveTeam_ShouldSetStatusToLeft_WhenMemberLeaves()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            await SeedMembershipAsync(db, teamId, memberId);
            var service = new TeamService(db);

            await service.LeaveTeamAsync(teamId, memberId);

            var membership = await db.Memberships
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == memberId);
            Assert.Equal(MembershipStatus.Left, membership.Status);
            Assert.NotNull(membership.LeftAt);
        }



        // DeleteTeamAsync-----------------------------------------------------------------------


        [Fact]
        public async Task DeleteTeam_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteTeamAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteTeam_ShouldThrowForbidden_WhenCallerIsNotCreator()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var memberId = await SeedMemberAsync(db);
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.DeleteTeamAsync(teamId, memberId));
        }

        [Fact]
        public async Task DeleteTeam_ShouldRemoveTeamAndRelatedData_WhenCreatorDeletes()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);

            // Seed a habit with an entry so we can verify cascade deletion
            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Habit",
                Goal = "Goal",
                Type = HabitType.Binary,
                Unit = "times",
                State = HabitState.Active,
                TeamId = teamId
            };
            db.Habits.Add(habit);
            db.HabitEntries.Add(new HabitEntry
            {
                Id = Guid.NewGuid(),
                HabitId = habit.Id,
                UserId = creatorId,
                Date = DateTime.UtcNow.Date,
                Status = EntryStatus.Logged,
                Notes = ""
            });
            await db.SaveChangesAsync();

            var service = new TeamService(db);
            await service.DeleteTeamAsync(teamId, creatorId);

            Assert.Null(await db.Teams.FindAsync(teamId));
            Assert.Empty(db.Habits.Where(h => h.TeamId == teamId));
            Assert.Empty(db.HabitEntries.Where(e => e.HabitId == habit.Id));
            Assert.Empty(db.Memberships.Where(m => m.TeamId == teamId));
        }



        // GetArchivedHabitsAsync-----------------------------------------------------------------------



        [Fact]
        public async Task GetArchivedHabits_ShouldThrowNotFound_WhenTeamDoesNotExist()
        {
            var db = CreateDb();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetArchivedHabitsAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldThrowForbidden_WhenUserIsNotMember()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);
            var outsiderId = Guid.NewGuid();
            var service = new TeamService(db);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.GetArchivedHabitsAsync(teamId, outsiderId));
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldReturnOnlyArchivedHabits_WhenCreatorRequests()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);

            // Seed one active and one archived habit
            db.Habits.Add(new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Active Habit",
                Goal = "Goal",
                Type = HabitType.Binary,
                Unit = "times",
                State = HabitState.Active,
                TeamId = teamId
            });
            db.Habits.Add(new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Archived Habit",
                Goal = "Goal",
                Type = HabitType.Binary,
                Unit = "times",
                State = HabitState.Archived,
                TeamId = teamId
            });
            await db.SaveChangesAsync();

            var service = new TeamService(db);
            var result = await service.GetArchivedHabitsAsync(teamId, creatorId);

            Assert.Single(result);
            Assert.Equal("Archived Habit", result[0].Name);
        }

        [Fact]
        public async Task GetArchivedHabits_ShouldReturnEmpty_WhenNoHabitsAreArchived()
        {
            var db = CreateDb();
            var creatorId = await SeedCreatorAsync(db);
            var teamId = await SeedTeamAsync(db, creatorId);

            db.Habits.Add(new Habit
            {
                Id = Guid.NewGuid(),
                Name = "Active Habit",
                Goal = "Goal",
                Type = HabitType.Binary,
                Unit = "times",
                State = HabitState.Active,
                TeamId = teamId
            });
            await db.SaveChangesAsync();

            var service = new TeamService(db);
            var result = await service.GetArchivedHabitsAsync(teamId, creatorId);

            Assert.Empty(result);
        }
    }
}