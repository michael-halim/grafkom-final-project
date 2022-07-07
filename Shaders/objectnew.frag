#version 330 core
#define NR_POINT_LIGHTS 3 // might change later, based on how many point lights we're using
struct Material{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shine_coefficient;
};
struct DirLight{
    // requires no position
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
struct PointLight {
    // requires no direction, goes everywhere
    vec3 position;
    // these are for dimming as distance grows
    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
struct SpotLight{
    // the most complicated of them all
    vec3  position;
    vec3  direction;
    // marks where the beam light ends
    float cutOff;
    // marks where the field light ends
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

out vec4 outputColor; // output after all the light calculations

in vec3 Normal;  // passed from vert
in vec3 FragPos; // passed from vert

//uniform vec3 objectColor; // passed from Asset3D.setFragVariable()
//uniform vec3 lightColor; // idk?
//uniform vec3 lightPos; // not used anymore
uniform vec3 viewPos; // passed from Asset3D.setFragVariable()
uniform Material material;
uniform DirLight dirLight;
uniform PointLight torchLight; // this for singular point light
uniform PointLight pointLight[NR_POINT_LIGHTS];

uniform SpotLight spotLight;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    //diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shine_coefficient); // CHANGE TO Ns !!!!!!
    //combine results
    vec3 ambient  = light.ambient  * material.diffuse * material.ambient;
    vec3 diffuse  = light.diffuse  * diff * material.diffuse;
    vec3 specular = light.specular * spec  * material.specular;
    return (ambient + diffuse + specular);
}
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    // to calculate diffuse, light comes in and we determine how much else comes out
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse  = light.diffuse  * diff * material.diffuse;

    //specular renders the light that's reflected, fully visible in that exact angle
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0),material.shine_coefficient); // CHANGE 256 to Ns VALUE OF THE MATERIAL!!!
    vec3 specular = light.specular * spec * material.specular;

    // ambience as is
    vec3 ambient  = light.ambient * material.diffuse  * material.ambient;

    //attenuation ( dimming of light as distance grows)
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    // multiply with "distance" formula
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{

    //diffuse shading
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(normal, lightDir), 0.0);

    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shine_coefficient); // CHANGE TO Ns!!!!!!!!!!!!
    //attenuation
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    //spotlight intensity
    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    //combine results
    vec3 ambient = light.ambient * material.diffuse * material.ambient;
    vec3 diffuse = light.diffuse * diff * material.diffuse;
    vec3 specular = light.specular * spec * material.specular;
    ambient  *= attenuation;
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}

// put comment on one of the lights if you don't wanna use them, else they all become dim
// also maybe don't use spotlights
void main()
{
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos -FragPos); // V is a vector from the base to the view position
    vec3 result = CalcDirLight(dirLight, norm, viewDir);

    // for multiple point lights
    result += vec3(0,0,0);
    for(int i=0; i<NR_POINT_LIGHTS; i++){
        result += CalcPointLight(pointLight[i], norm, FragPos, viewDir);
    }
    result += CalcPointLight(torchLight, norm, FragPos, viewDir);
    result += CalcSpotLight(spotLight, norm, FragPos, viewDir);
    outputColor = vec4(result, 1);

}