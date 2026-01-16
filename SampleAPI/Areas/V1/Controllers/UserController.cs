using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAPI.Interfaces;
using SampleAPI.Models;

namespace SampleAPI.Areas.V1.Controllers;

/// <summary>
/// ユーザー管理コントローラー
/// </summary>
[ApiController]
[Area("V1")]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// すべてのユーザーを取得
    /// </summary>
    /// <returns>ユーザーリスト</returns>
    /// <response code="200">成功</response>
    /// <response code="401">認証エラー</response>
    /// <response code="500">サーバーエラー</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<UserResponseModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<UserResponseModel>>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponseModel<IEnumerable<UserResponseModel>>.SuccessResponse(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// ユーザーIDでユーザーを取得
    /// </summary>
    /// <param name="id">ユーザーID</param>
    /// <returns>ユーザー情報</returns>
    /// <response code="200">成功</response>
    /// <response code="401">認証エラー</response>
    /// <response code="404">ユーザーが見つかりません</response>
    /// <response code="500">サーバーエラー</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseModel<UserResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseModel<UserResponseModel>>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(ApiResponseModel<UserResponseModel>.ErrorResponse($"User with ID {id} not found"));
        }

        return Ok(ApiResponseModel<UserResponseModel>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// 新しいユーザーを作成
    /// </summary>
    /// <param name="request">ユーザー作成リクエスト</param>
    /// <returns>作成されたユーザー情報</returns>
    /// <response code="201">作成成功</response>
    /// <response code="400">リクエストエラー</response>
    /// <response code="401">認証エラー</response>
    /// <response code="500">サーバーエラー</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseModel<UserResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseModel<UserResponseModel>>> CreateUser([FromBody] UserRequestModel request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponseModel<UserResponseModel>.ErrorResponse("Validation failed", errors));
        }

        var user = await _userService.CreateUserAsync(request);
        
        return CreatedAtAction(
            nameof(GetUserById),
            new { id = user.Id },
            ApiResponseModel<UserResponseModel>.SuccessResponse(user, "User created successfully"));
    }

    /// <summary>
    /// ユーザー情報を更新
    /// </summary>
    /// <param name="id">ユーザーID</param>
    /// <param name="request">ユーザー更新リクエスト</param>
    /// <returns>更新されたユーザー情報</returns>
    /// <response code="200">更新成功</response>
    /// <response code="400">リクエストエラー</response>
    /// <response code="401">認証エラー</response>
    /// <response code="404">ユーザーが見つかりません</response>
    /// <response code="500">サーバーエラー</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseModel<UserResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseModel<UserResponseModel>>> UpdateUser(int id, [FromBody] UserRequestModel request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponseModel<UserResponseModel>.ErrorResponse("Validation failed", errors));
        }

        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(ApiResponseModel<UserResponseModel>.SuccessResponse(user, "User updated successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponseModel<UserResponseModel>.ErrorResponse($"User with ID {id} not found"));
        }
    }

    /// <summary>
    /// ユーザーを削除
    /// </summary>
    /// <param name="id">ユーザーID</param>
    /// <returns>削除結果</returns>
    /// <response code="200">削除成功</response>
    /// <response code="401">認証エラー</response>
    /// <response code="404">ユーザーが見つかりません</response>
    /// <response code="500">サーバーエラー</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponseModel<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponseModel<bool>.ErrorResponse($"User with ID {id} not found"));
        }

        return Ok(ApiResponseModel<bool>.SuccessResponse(true, "User deleted successfully"));
    }
}
