using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HookShooter : MonoBehaviour
{
    [SerializeField] private GameObject HookPrefab;
    [SerializeField] private GameObject RopePrefab;
    [SerializeField] private Rigidbody PlayerRigidbody;
    private Vector3 localPos;
    [SerializeField] private int RopeQuality = 100;
    [SerializeField] public float MaxRopeLength = 200;
    public static float ForceMultiplier = 1;
    [SerializeField] private float StartVelocity = 50;
    public bool Retracted = true;
    private List<RopePart> Parts = new List<RopePart>();

    private struct RopePart
    {
        private GameObject self;
        public RopePart(Vector3 Pos, Quaternion Rot, GameObject Prefab, Transform Parent, float Quality)
        {
             self = GameObject.Instantiate(Prefab, Pos, Rot, Parent);
        }
        public void Destroy(){ GameObject.Destroy(self); }
        public Vector3 getPos(){ return self.transform.position; }
        private Vector3 getScale(){ return self.transform.localScale; }
        private Quaternion getRot(){ return self.transform.rotation; }
        private void setPos(Vector3 Pos){ self.transform.position = Pos; }
        private void setRot(Quaternion Rot){ self.transform.rotation = Rot; }
        private void setScale(Vector3 Scale){ self.transform.localScale = Scale; }
        public Rigidbody getRigidbody(){ return self.GetComponent<Rigidbody>(); }
        public void setIsTrigger(bool value){ foreach(Collider C in self.GetComponentsInChildren<Collider>()) C.isTrigger = value; }
        public void setParent(Transform P){ self.transform.SetParent(P,true); }
        public void AddForce(Vector3 Dir, float Speed, ForceMode FM){ self.GetComponent<Rigidbody>()?.AddForce(Speed*Dir, FM); }
        public void Evaluate(Vector3[] Points)
        {
            if (Points.Length > 2 || Points.Length == 0) { Debug.Log("Rope Evaluation Error: faulty TargetList"); return; }
            float AvgDist = 0;
            Vector3 AvgPos = Vector3.zero;
            foreach(Vector3 T in Points) 
            {
                AvgPos += T;
                AvgDist += Vector3.Magnitude(getPos()-T);
            }
            AvgPos /= Points.Length;
            AvgDist = Mathf.Min(AvgDist/Points.Length, 100);
            Rigidbody RB = self.GetComponent<Rigidbody>();
            RB.AddForce(ForceMultiplier * (AvgPos-getPos()), ForceMode.Impulse);
            Vector3.ClampMagnitude(RB.velocity, (AvgPos-getPos()).magnitude / Time.deltaTime);
            if (Points.Length == 2) setRot(Quaternion.LookRotation(Points[0]-Points[1]));
            else setRot(Quaternion.LookRotation(getPos()-Points[0]));
            setScale(new Vector3(0.3f,0.3f,0) + Vector3.forward * 0.75f*AvgDist);
        }

    }

    public void Start()
    {
        localPos = transform.position - PlayerRigidbody.transform.position;
    }
    public void Update()
    {
        transform.position = PlayerRigidbody.transform.position + localPos;
        if (Retracted) {return;}
        Parts[0].Evaluate(new Vector3[]{transform.position, Parts[1].getPos()});
        float RopeLength = 0;
        for(int i = 1; i < Parts.Count - 1; i++) 
        {
            RopeLength += (Parts[i].getPos() - Parts[i-1].getPos()).magnitude;
            Parts[i].Evaluate(new Vector3[]{Parts[i-1].getPos(), Parts[i+1].getPos()});
        }
        //PlayerRigidbody.AddForce(ForceMultiplier * (Parts[0].getPos()-transform.position), ForceMode.Impulse);
        
    }

    public void ShootRope(Vector3 Dir)
    {
        if (!Retracted) return;
        Retracted = false;
        Parts.Clear();
        for(int i = 1; i < RopeQuality - 1; i++) 
        {
            Parts.Add(new RopePart(i*Dir.normalized/RopeQuality + transform.position, Quaternion.LookRotation(Dir.normalized), RopePrefab, transform, RopeQuality));
            Parts.Last().AddForce(Dir.normalized, StartVelocity * Mathf.Log(1+i)/Mathf.Log(RopeQuality + 1), ForceMode.VelocityChange);
        }
        Parts.Add(new RopePart(Dir.normalized, Quaternion.LookRotation(Dir.normalized), HookPrefab, transform, RopeQuality));
        Parts.Last().AddForce(Dir.normalized, StartVelocity, ForceMode.VelocityChange);
    }

    public void TightenRope(float value)
    {
        Parts.Last().setParent(transform.parent);
        Rigidbody RB = Parts.Last().getRigidbody();
        RB.useGravity = false;
        RB.constraints = RigidbodyConstraints.FreezePosition;
        RB.velocity = Vector3.zero;
        for(int i = 0; i < Parts.Count - 1; i++) 
        {
            if (i%5==0 && i!=0) Parts[i].setIsTrigger(false);
            Parts[i].getRigidbody().useGravity = false;
        }
        Parts[0].AddForce((Parts[0].getPos()-transform.position).normalized, value, ForceMode.Impulse);
    }

    public void ResetRope()
    {
        foreach(RopePart RP in Parts) RP.Destroy();
        Parts.Clear();
        Retracted = true;
    }
}
