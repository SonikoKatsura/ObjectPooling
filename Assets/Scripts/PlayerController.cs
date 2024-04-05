using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour {

    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] int bulletSpeed = 20;

    // Referencia al LineRenderer
    public LineRenderer rayoVisual;

    void Start() {

        // Obtenemos la referencia al CharacterController
        characterController = GetComponent<CharacterController>();

        // Ocultamos el mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ocultamos el rayo
        rayoVisual.enabled = false;
    }

    void Update() {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded) {
            moveDirection.y = jumpPower;
        } else {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded) {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.R) && canMove) {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        } else {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove) {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Control de disparos
        if (Input.GetMouseButtonDown(0)) {

            // Creamos el Raycast que apunte desde la cámara al centro de la pantalla
            Ray rayo = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;


            // -------------------------------------------------
            // Primero se verifica si el rayo ha golpeado algo
            // -------------------------------------------------
            if (Physics.Raycast(rayo, out hit)) {

                // Obtener el punto de impacto del rayo visual
                Vector3 puntoImpacto = hit.point;

                // Mostrar el rayo desde la punta de la pistola hasta el lugar de impacto
                rayoVisual.enabled = true;
                rayoVisual.SetPosition(0, firePoint.position);
                rayoVisual.SetPosition(1, puntoImpacto);

                // Instanciar la bala y establecer su dirección hacia el punto de impacto
                /*GameObject balaImpacto = Instantiate(bulletPrefab, firePoint);
                balaImpacto.transform.SetParent(null);
                Vector3 direccionImpacto = (puntoImpacto - firePoint.transform.position).normalized;
                balaImpacto.GetComponent<Rigidbody>().velocity = direccionImpacto * bulletSpeed;*/
                // Instanciar la bala y establecer su dirección hacia el punto lejano
                /*GameObject bullet = Instantiate(bulletPrefab, firePoint);
                bullet.transform.SetParent(null);*/

                // Obtener la dirección hacia el punto final del rayo visual
                Vector3 puntoLejano = rayo.GetPoint(100);
                Vector3 direccion = (puntoLejano - firePoint.transform.position).normalized;
                //bullet.GetComponent<Rigidbody>().velocity = direccion * bulletSpeed;

                // ----------------------------------------------------------------------------------
                // Utilizamos una bala del Object Pool (sustituye a la instanciación si hay impacto)
                // ----------------------------------------------------------------------------------
                GameObject bullet = ObjectPool.instance.GetPooledObject();

                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position; // Colocamos el objeto en la posición
                    bullet.SetActive(true); // Activamos el objeto del pool
                    Vector3 direccionImpacto = (puntoImpacto - firePoint.transform.position).normalized;
                    bullet.GetComponent<Rigidbody>().velocity = direccionImpacto * bulletSpeed;
                }


            }
            else {

                // --------------------------------------------------------------
                // Si el rayo no ha golpeado nada, simplemente se extiende lejos
                // --------------------------------------------------------------

                // Si el rayo no golpea nada, extenderlo lejos
                rayoVisual.SetPosition(0, firePoint.position);
                rayoVisual.SetPosition(1, rayo.GetPoint(100));

                // Mostramos el rayo
                rayoVisual.enabled = true;

                // Obtener la dirección hacia el punto final del rayo visual
                Vector3 puntoLejano = rayo.GetPoint(100);
                Vector3 direccion = (puntoLejano - firePoint.transform.position).normalized;

                // -------------------------------------------------------------------------------------
                // Utilizamos una bala del Object Pool (sustituye a la instanciación si no hay impacto)
                // -------------------------------------------------------------------------------------
                GameObject bullet = ObjectPool.instance.GetPooledObject();
                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position; // Colocamos el objeto en la posición
                    bullet.SetActive(true); // Activamos el objeto del pool
                    bullet.GetComponent<Rigidbody>().velocity = direccion * bulletSpeed;
                }
                /*
                // Instanciar la bala y establecer su dirección hacia el punto lejano
                GameObject bullet = Instantiate(bulletPrefab, firePoint);
                bullet.transform.SetParent(null);
                 */          

            }

        } else {
            // Si se deja de hacer clic
            if (Input.GetMouseButtonUp(0)) {

                // Ocultamos el rayo
                rayoVisual.enabled = false;

            }
        }

    }

}