using API.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace API.Endpoint;

public static class UserDetailEndpoint
{

	private static async Task<IResult> GetUserDetailAsync(
		[FromRoute] string userId,
		[FromServices] WebAuthenticationDbContext dbContext,
		CancellationToken cancellation = default)
	{
		var user = await dbContext.UserDetails
			.AsNoTracking()
			.Include(user => user.KanBanSections)
				.ThenInclude(section => section.KanBanTaskItems)
			.FirstOrDefaultAsync(user => user.Id == userId, cancellation);

		return user is null
			? Results.NotFound()
			: Results.Ok(user);
	}

	private static async Task<IResult> UpdateKanBanAsync(
		[FromForm] UserDetail detail,
		[FromServices] WebAuthenticationDbContext dbContext,
		CancellationToken cancellation = default)
	{
		var user = await dbContext.UserDetails.FirstOrDefaultAsync(user => user.Id == detail.Id, cancellationToken: cancellation);

		if (user is null)
		{
			return Results.NotFound();
		}

		user.KanBanSections.Clear();

		await dbContext.SaveChangesAsync(cancellation);

		foreach (var section in detail.KanBanSections)
		{
			user.KanBanSections.Add(section);
		}

		await dbContext.SaveChangesAsync(cancellation);

		return Results.Ok(user);
	}
}
