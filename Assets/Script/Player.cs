using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Rigidbody2D _rb;
    private PlayerAction _playerActions;

    [SerializeField] private float _speed = 15f;
    
    private bool facingRight = true;
    private Vector2 _moveInput;


    [SerializeField] [Range(1f, 5f)]private float _jumpFallGravityMultiplier;
    [SerializeField] private float _jumpSpeed = 3f;
    [SerializeField] private float _disableGCTime;
    [SerializeField] private float _groundCheckHeight;
    [SerializeField] private LayerMask _ground;

    private Vector2 _boxCenter;
    private Vector2 _boxSize;
    private bool _jumping;
    private float _initialGravityScale;
    private bool _groundCheckEnabled = true;
    private WaitForSeconds _wait;
    private BoxCollider2D _pol;
    private void Awake()
    {
        _playerActions = new PlayerAction();

        _wait = new WaitForSeconds(_disableGCTime);
        
        _animator= GetComponent<Animator>();
        _pol =  GetComponent<BoxCollider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _pol = GetComponent<BoxCollider2D>();

        _playerActions.Player_Map.Jump.performed += Jump_performed;

    }

    private void OnEnable()
    {
        _playerActions.Player_Map.Enable();
    }

    private void OnDisable()
    {
        _playerActions.Player_Map.Disable();
    }
    private void Jump_performed(InputAction.CallbackContext context)
    {
        if(isGrounded())
        {
            _rb.velocity += Vector2.up * _jumpSpeed;
            _jumping = true;
            StartCoroutine(EnableGroundCheckAfterJump());
        }
    }

    private bool isGrounded()
    {
        _boxCenter = new Vector2(_pol.bounds.center.x, _pol.bounds.center.y) +
            (Vector2.down * (_pol.bounds.extents.y + (_groundCheckHeight / 2f)));

        _boxSize = new Vector2(_pol.bounds.size.x, _groundCheckHeight);

        var groundBox = Physics2D.OverlapArea(_boxCenter, _boxSize, 0, _ground);

        if (groundBox != null)
            return true;
        return false;
    }

    private IEnumerator EnableGroundCheckAfterJump()
    {
        _groundCheckEnabled = false;
        yield return _wait;
        _groundCheckEnabled = true;
    }

    private void HandleGravity()
    {
        if (_groundCheckEnabled && isGrounded())
        {
            _jumping = false;
        }
        else if (_jumping && _rb.velocity.y < 0f)
        {
            _rb.gravityScale = _initialGravityScale * _jumpFallGravityMultiplier;
        }
        else
        {
            _rb.gravityScale = _initialGravityScale;    
        }    
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    private void HandleMovement()
    {
        _moveInput = _playerActions.Player_Map.Movement.ReadValue<Vector2>();
        _moveInput.y = 0f;
        _rb.velocity = _moveInput * _speed;
    }
    
    private void FixedUpdate()
    { 
        HandleMovement();
        _rb.velocity = new Vector2(_rb.velocity.x , _rb.velocity.y);

        HandleGravity();
    }

    private void OnDrawGizmos()
    {
        if (_jumping)
            Gizmos.color = Color.red;
        else 
            Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_boxCenter, _boxSize);
    }

    private void Update()
    {
        if (facingRight == false && _moveInput.x > 0)
        {
            Flip();
        }
        else if (facingRight == true && _moveInput.x < 0)
        {
            Flip();
        }
    }
}
