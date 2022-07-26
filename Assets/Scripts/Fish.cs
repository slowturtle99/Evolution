using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    /* Setting */
    public FishSetting setting;

    public Gene gene = new Gene();

    /* States */
    public float fat = 1.0f;
    public float muscle = 1.0f;
    public float age = 0.0f;
    public string status = "None";
    public float speed_sp;
    public float maxSpeed = 4.0f;
    public float idleSpeed = 1.0f;

    /* Physical States */
    public float mass;
    public Vector3 heading_, P_, V_, W_;
    public float speed_;
    private float boundRadius;

    /* View */
    public float viewingRange;

    /* Flocking */
    private Vector3 avgFlockHeading;
    private Vector3 offsetToFlockCetre;
    private Vector3 seperationHeading;
    private Vector3 offsetToMate;
    private Vector3 offsetToPrey;
    private Vector3 offsetToPredator;

    /* Components */
    private Rigidbody rb;
    private CapsuleCollider cl;
    private Renderer rd;

    public class Gene
    {
        public Color color;
        public float adultMass = 0.4f;
        public float idealMuscleRatio = 0.5f;

        public Gene()
        {
            SetColor();
        }

        public Gene(Gene gene)
        {
            color = gene.color;
            adultMass = gene.adultMass;
            idealMuscleRatio = gene.idealMuscleRatio;
            SetColor();
        }

        public void Mutate(Gene otherGene, float mutationRate){
            adultMass = (adultMass + otherGene.adultMass)/2.0f;
            adultMass = (Random.Range(0.0f,1.0f)<mutationRate) ? Mathf.Clamp(adultMass + Random.Range(-0.1f,0.1f), 0.1f, 2.0f) : adultMass;
            idealMuscleRatio = (idealMuscleRatio + otherGene.idealMuscleRatio)/2.0f;
            idealMuscleRatio = (Random.Range(0.0f,1.0f)<mutationRate) ? Mathf.Clamp(idealMuscleRatio + Random.Range(-0.1f,0.1f), 0.1f, 0.9f) : idealMuscleRatio;
            SetColor();
        }
        
        private void SetColor(){
            float hue=0;
            hue += adultMass;
            hue += idealMuscleRatio;
            hue = hue % 1.0f;
            // Debug.Log(hue);
            color = Color.HSVToRGB(hue, 1.0f, 1.0f);
        }

        public bool isSameSpecies(Gene otherGene, float limit)
        {
            return Mathf.Abs(adultMass-otherGene.adultMass) < limit
            && Mathf.Abs(idealMuscleRatio-otherGene.idealMuscleRatio) < limit;
        }

    }

    void Start() {
        viewingRange = setting.maxViewingRange;

        rb = GetComponent<Rigidbody>();
        cl = GetComponent<CapsuleCollider>();
        rd = GetComponentInChildren<Renderer>();
        
        GameObjectUpdate();
        StateUpdate();
    }

    void Update() {
        GameObjectUpdate();
        StateUpdate();
        
        View();
        Control();
    }

    private void StateUpdate() {
        age += Time.deltaTime;

        /* Get Physical States*/
        heading_ = transform.forward;
        P_ = transform.position;
        V_ = rb.velocity;
        W_ = rb.angularVelocity;
        speed_ = V_.magnitude;
        boundRadius = cl.radius*transform.localScale.z;

        /* Energy & Speed */
        fat -= setting.basalMetabolismCoeff*muscle;
        fat -= setting.dragCoeff*speed_*speed_*boundRadius*boundRadius;
        mass = fat + muscle;

        maxSpeed = setting.maxSpeedCoeff*Mathf.Sqrt(muscle/boundRadius/boundRadius);
        idleSpeed = setting.idleSpeedCoeff*muscle/boundRadius/boundRadius;

        /* Dying Condition */
        if(fat<0) {
            Debug.Log("Starve to death.");
            Die();
        }

        if(age>setting.maxAge) {
            Debug.Log("Max age.");
            Die();
        }
    }

    private void GameObjectUpdate() {
        float scale = Mathf.Pow(mass/1000.0f/setting.fishPrefabDefaultVolume,1.0f/3.0f);
        transform.localScale = new Vector3(1.0f,1.0f,1.0f)*scale;
        rb.mass = mass;
        rd.material.color = gene.color;
    }

    private void Control() {
        speed_sp = idleSpeed;
        Vector3 acceleration = setting.alignWeight*SteerTowards(avgFlockHeading, speed_sp)
        + setting.cohesionWeight*SteerTowards(offsetToFlockCetre, idleSpeed)
        + setting.seperationWeight*SteerTowards(seperationHeading, idleSpeed);
        if(IsHeadingForCollision()){
            acceleration += setting.obstacleAvoidWeight*SteerTowards(ObstacleRays(), speed_sp);
        }
        rb.velocity += acceleration*Time.deltaTime;
        transform.forward = V_.normalized;
    }

    private void View() {

        Collider[] planktonHitColliders = Physics.OverlapSphere(P_, boundRadius+0.05f, setting.PlanktonMask);
        foreach (var planktonHitCollider in planktonHitColliders)
        {
            Plankton otherPlankton = planktonHitCollider.gameObject.GetComponent<Plankton>();
            fat += setting.predationEfficiency*otherPlankton.mass;
            otherPlankton.Die();
        }


        int numSame = 0;
        float minMateDist = setting.maxViewingRange;
        float minPreyDist = setting.maxViewingRange;
        avgFlockHeading = Vector3.zero;
        offsetToFlockCetre = Vector3.zero;
        seperationHeading = Vector3.zero;
        offsetToMate = Vector3.zero;
        offsetToPrey = Vector3.zero;
        offsetToPredator = Vector3.zero;


        Collider[] hitColliders = Physics.OverlapSphere(P_, viewingRange, setting.FishMask);
        foreach (var hitCollider in hitColliders){
            Fish otherFish = hitCollider.gameObject.GetComponent<Fish>();

            if(this == otherFish){
                continue;
            }

            Vector3 offset = otherFish.P_-P_;
            float dist = offset.magnitude;
            
            if(isSameSpecies(otherFish)){
                numSame++;
                avgFlockHeading += otherFish.V_;
                offsetToFlockCetre += offset;
                seperationHeading -= offset/dist/dist;

                if(isReproductive() && otherFish.isReproductive()){
                    if(dist < minMateDist){
                        minMateDist = dist;
                        offsetToMate = offset;
                    }
                    if(dist < (boundRadius+otherFish.boundRadius)*2.0f){
                        Debug.Log("Reproduce.");
                        Reproduce(otherFish.gene);
                    }
                }
            } else {
                if(isPrey(otherFish)){
                    if(dist < minPreyDist){
                        minPreyDist = dist;
                        offsetToPrey = offset;
                    }
                }
                if(otherFish.isPrey(this)){
                    offsetToPredator -= offset/dist/dist;
                }
            }
            
        }

        if(numSame != 0){
            avgFlockHeading /= numSame;
            offsetToFlockCetre /= numSame;
            seperationHeading /= numSame;
        }

        // if(numSame < 5){
        //     viewingRange *=1.5f;
        // }
        // else{
        //     viewingRange /=1.5f;
        // }
        // viewingRange = Mathf.Clamp(viewingRange, boundRadius*5.0f, setting.maxViewingRange);

    }

    private bool IsHeadingForCollision() {
        RaycastHit hit;
        if (Physics.SphereCast (P_, boundRadius, V_, out hit, V_.magnitude * setting.obstacleAvoidWeight + boundRadius, setting.obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    private Vector3 ObstacleRays() {
        Vector3[] rayDirections = FishHelper.directions;
        float rayLength = V_.magnitude * setting.obstacleAvoidWeight + boundRadius*6;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = Quaternion.FromToRotation(Vector3.forward, V_)*rayDirections[i];
            Ray ray = new Ray (P_, dir);
            if (!Physics.SphereCast(ray, boundRadius, rayLength, setting.obstacleMask)) {
                return dir;
            }
        }
        return -V_;
    }

    void Reproduce(Gene otherGene){
        Gene tempGene = new Gene(gene);
        tempGene.Mutate(otherGene, setting.mutationRate);
        Debug.Log("Reproduce!");

        Vector3 pos = transform.position + Random.onUnitSphere*boundRadius*Random.Range(2,3);
        Fish offspring = Instantiate(setting.FishPrefab);

        /* set basic states */
        offspring.transform.position = P_;
        
        offspring.fat = setting.childAdultRatio*gene.adultMass*(1.0f - gene.idealMuscleRatio);
        offspring.muscle = setting.childAdultRatio*gene.adultMass*gene.idealMuscleRatio;
        offspring.gene = tempGene;

        /* remove fat */
        fat -= (offspring.fat+offspring.muscle)/setting.birthEfficiency;
    }

    public void Die() {
        Destroy(gameObject);
    }

    Vector3 SteerTowards (Vector3 vector, float speedSetPoint) {
        Vector3 v = vector.normalized * speedSetPoint - V_;
        return Vector3.ClampMagnitude (v, setting.maxSteerForce);
    }
    
    bool isSameSpecies(Fish otherFish) {
        return gene.isSameSpecies(otherFish.gene, setting.geneDiffLimit);
    }

    bool isReproductive() {
        float requiredFat = setting.childAdultRatio*gene.adultMass;
        return muscle > gene.adultMass*gene.idealMuscleRatio 
        && fat > gene.adultMass*(1.0f - gene.idealMuscleRatio) + requiredFat;
    }

    public bool isPrey(Fish otherFish){
        return mass*setting.predationMassRatio > otherFish.mass;
    }

}
