using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Goomba : MonoBehaviour, IRestartGameElement
{
    public float m_KillTime = 0.5f;
    public float m_KillScale = 0.2f;
    public float m_Alert = 2.0f;
    public Transform m_Mario;
    NavMeshPath m_AttackPath;
    public float m_Speed = 5.0f;
    CharacterController m_CharacterController;
    NavMeshAgent m_NavMeshAgent;
    
    public float m_EyesHeight;
    public float m_VisualConeAngle = 60.0f;
    public LayerMask m_SightLayerMask;
    public float m_SightDistance = 8.0f;
    public float m_EyesPlayerHeight;

    public List<Transform> m_PatrolTargets;
    int m_CurrentPatrolTargetId = 0;

    public enum TStates
    {
        PATROL,
        ALERT,
        ATTACK
    }

    TStates m_CurrentState;

    private void Awake()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        m_CurrentState = TStates.PATROL;
        GameController.GetGameController().AddRestartGameElement(this);
        m_AttackPath = new NavMeshPath();
    }

    private void Update()
    {
        switch (m_CurrentState)
        {
            case TStates.PATROL:
                UpdatePatrolState();
                break;
            case TStates.ALERT:
                UpdateAlertState();
                break;
            case TStates.ATTACK:
                UpdateAttackState();
                break;
        }
    }
    void UpdatePatrolState()
    {
        if (SeesPlayer())
        {
            m_NavMeshAgent.isStopped = true;
            m_CurrentState = TStates.ALERT;
        }
        if (PatrolTargetPositionArrived())
        {
            MoveToNextPatrolPosition();
        }
    }
    void UpdateAlertState()
    {
        StartCoroutine(Surprise());
    }
    void UpdateAttackState()
    {
        OnAttack();
    }

    public void Kill()
    {
        transform.localScale = new Vector3(1.0f, m_KillScale, 1.0f);
        StartCoroutine(Hide());
    }
    IEnumerator Hide()
    {
        yield return new WaitForSeconds(m_KillTime);
        gameObject.SetActive(false);

    }
    IEnumerator Surprise()
    {
        yield return new WaitForSeconds(m_Alert);
        m_CurrentState = TStates.ATTACK;
        //animacion de goomba saltando
        
    }
    public void OnAttack()
    {
        Vector3 l_playerPosition = GameController.GetGameController().GetPlayer().transform.position;
        Vector3 l_dronePosition = transform.position;

        float distance = Vector3.Distance(l_playerPosition, l_dronePosition);
        
        transform.LookAt(l_playerPosition);
        transform.position = Vector3.MoveTowards(l_dronePosition, l_playerPosition, m_NavMeshAgent.speed * Time.deltaTime);
        
    }
    public void RestartGame()
    {
        gameObject.SetActive(true);
    }

    bool PatrolTargetPositionArrived()
    {
        return !m_NavMeshAgent.hasPath && !m_NavMeshAgent.pathPending && m_NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete;
    }

    void MoveToNextPatrolPosition()
    {
        ++m_CurrentPatrolTargetId;
        if (m_CurrentPatrolTargetId >= m_PatrolTargets.Count)
            m_CurrentPatrolTargetId = 0;
        m_NavMeshAgent.destination = m_PatrolTargets[m_CurrentPatrolTargetId].position;
    } 
    bool SeesPlayer()
    {
        Vector3 l_playerPosition = GameController.GetGameController().GetPlayer().transform.position;
        Vector3 l_DirectionToPlayerXZ = l_playerPosition - transform.position;
        l_DirectionToPlayerXZ.y = 0.0f;
        l_DirectionToPlayerXZ.Normalize();
        Vector3 l_ForwardXZ = transform.forward;
        l_ForwardXZ.y = 0.0f;
        l_ForwardXZ.Normalize();

        Vector3 l_EyesPosition = transform.position + Vector3.up * m_EyesHeight;
        Vector3 l_PlayerEyesPosition = l_playerPosition + Vector3.up * m_EyesPlayerHeight;
        Vector3 l_Direction = l_PlayerEyesPosition - l_EyesPosition;

        float l_Lenght = l_Direction.magnitude;
        l_Direction /= l_Lenght;


        Ray l_Ray = new Ray(l_EyesPosition, l_Direction);
        
        
        return Vector3.Dot(l_ForwardXZ, l_DirectionToPlayerXZ) > Mathf.Cos(m_VisualConeAngle * Mathf.Deg2Rad / 2.0f) && !Physics.Raycast(l_Ray, l_Lenght, m_SightLayerMask.value);
    }
}

   
