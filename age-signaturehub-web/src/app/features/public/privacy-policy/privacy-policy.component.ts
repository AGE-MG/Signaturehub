import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

interface Section {
  id: string;
  title: string;
  icon: string;
  content: string[];
}

@Component({
  selector: 'app-privacy-policy',
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule
  ]
})
export class PrivacyPolicyComponent implements OnInit {
  lastUpdate = new Date('2026-03-13');

  sections: Section[] = [
    {
      id: 'introducao',
      title: 'Introdução',
      icon: 'info',
      content: [
        'Esta Política de Privacidade foi elaborada em conformidade com a Lei Federal n. 12.965 de 23 de abril de 2014 (Marco Civil da Internet) e com a Lei Federal n. 13.709, de 14 de agosto de 2018 (Lei Geral de Proteção de Dados Pessoais - LGPD).',
        'Esta Política de Privacidade poderá ser atualizada em decorrência de eventual atualização normativa, razão pela qual se convida o usuário a consultar periodicamente esta seção.',
        'O SignatureHub se compromete a cumprir as normas previstas na LGPD, respeitando os princípios e direitos dos titulares de dados pessoais.'
      ]
    },
    {
      id: 'principios',
      title: 'Princípios da LGPD',
      icon: 'balance',
      content: [
        'O site se compromete a cumprir as normas previstas na Lei Geral de Proteção de Dados (LGPD), e respeitar os princípios dispostos no Art. 6º:',
        'I – <strong>Finalidade:</strong> realização do tratamento para propósitos legítimos, específicos, explícitos e informados ao titular, sem possibilidade de tratamento posterior de forma incompatível com essas finalidades.',
        'II – <strong>Adequação:</strong> compatibilidade do tratamento com as finalidades informadas ao titular, de acordo com o contexto do tratamento.',
        'III – <strong>Necessidade:</strong> limitação do tratamento ao mínimo necessário para a realização de suas finalidades, com abrangência dos dados pertinentes, proporcionais e não excessivos em relação às finalidades do tratamento de dados.',
        'IV – <strong>Livre acesso:</strong> garantia, aos titulares, de consulta facilitada e gratuita sobre a forma e a duração do tratamento, bem como sobre a integralidade de seus dados pessoais.',
        'V – <strong>Qualidade dos dados:</strong> garantia, aos titulares, de exatidão, clareza, relevância e atualização dos dados, de acordo com a necessidade e para o cumprimento da finalidade de seu tratamento.',
        'VI – <strong>Transparência:</strong> garantia, aos titulares, de informações claras, precisas e facilmente acessíveis sobre a realização do tratamento e os respectivos agentes de tratamento, observados os segredos comercial e industrial.',
        'VII – <strong>Segurança:</strong> utilização de medidas técnicas e administrativas aptas a proteger os dados pessoais de acessos não autorizados e de situações acidentais ou ilícitas de destruição, perda, alteração, comunicação ou difusão.',
        'VIII – <strong>Prevenção:</strong> adoção de medidas para prevenir a ocorrência de danos em virtude do tratamento de dados pessoais.',
        'IX – <strong>Não discriminação:</strong> impossibilidade de realização do tratamento para fins discriminatórios ilícitos ou abusivos.',
        'X – <strong>Responsabilização e prestação de contas:</strong> demonstração, pelo agente, da adoção de medidas eficazes e capazes de comprovar a observância e o cumprimento das normas de proteção de dados pessoais e, inclusive, da eficácia dessas medidas.'
      ]
    },
    {
      id: 'dados-coletados',
      title: 'Dados Coletados',
      icon: 'storage',
      content: [
        'O SignatureHub coleta e trata os seguintes tipos de dados pessoais:',
        '<strong>Dados de Identificação:</strong> Nome completo, CPF, RG, endereço de e-mail, telefone.',
        '<strong>Dados Profissionais:</strong> Cargo, setor, matrícula funcional (para servidores da AGE-MG).',
        '<strong>Dados de Certificado Digital:</strong> Informações do certificado ICP-Brasil (quando aplicável), incluindo nome do titular, CPF, validade, emissor.',
        '<strong>Dados de Navegação:</strong> Endereço IP, tipo de navegador, data e hora de acesso, páginas visitadas.',
        '<strong>Dados de Assinatura:</strong> Metadados da assinatura incluindo IP, dispositivo, localização aproximada, data e hora da ação.',
        'Todos os dados são coletados com finalidades específicas e mediante consentimento do titular, exceto quando dispensado por lei.'
      ]
    },
    {
      id: 'finalidade',
      title: 'Finalidade do Tratamento',
      icon: 'flag',
      content: [
        'Os dados pessoais coletados têm as seguintes finalidades:',
        '<strong>Identificação e Autenticação:</strong> Garantir que apenas usuários autorizados acessem o sistema.',
        '<strong>Processamento de Assinaturas:</strong> Viabilizar a assinatura digital e eletrônica de documentos.',
        '<strong>Auditoria e Compliance:</strong> Manter registros de todas as operações para fins de auditoria legal e administrativa.',
        '<strong>Comunicação:</strong> Enviar notificações sobre documentos pendentes, lembretes e atualizações do sistema.',
        '<strong>Melhoria do Serviço:</strong> Análise de uso para aprimoramento da plataforma.',
        '<strong>Segurança:</strong> Prevenir fraudes, acessos não autorizados e outras atividades maliciosas.',
        'O tratamento de dados é realizado apenas para as finalidades informadas e autorizadas, sendo vedado o uso para outros propósitos incompatíveis.'
      ]
    },
    {
      id: 'compartilhamento',
      title: 'Compartilhamento de Dados',
      icon: 'share',
      content: [
        'O SignatureHub não comercializa, aluga ou vende dados pessoais a terceiros.',
        'Os dados podem ser compartilhados apenas nas seguintes situações:',
        '<strong>Com os Signatários:</strong> Dados necessários para o processo de assinatura são compartilhados apenas com os signatários autorizados do documento.',
        '<strong>Prestadores de Serviço:</strong> Fornecedores de infraestrutura (hospedagem, backup) que atuam como operadores de dados, sempre mediante contrato e obrigações de confidencialidade.',
        '<strong>Autoridades Competentes:</strong> Quando exigido por lei, ordem judicial ou regulamentação governamental.',
        '<strong>Proteção de Direitos:</strong> Para proteção dos direitos da AGE-MG, prevenção de fraudes ou investigação de atividades suspeitas.',
        'Em todos os casos de compartilhamento, são adotadas medidas técnicas e organizacionais para proteção dos dados.'
      ]
    },
    {
      id: 'seguranca',
      title: 'Segurança da Informação',
      icon: 'security',
      content: [
        'O SignatureHub implementa as seguintes medidas de segurança:',
        '<strong>Criptografia:</strong> Dados em trânsito protegidos por TLS 1.3 e dados em repouso criptografados com AES-256.',
        '<strong>Controle de Acesso:</strong> Autenticação forte com controle baseado em funções (RBAC) e princípio do menor privilégio.',
        '<strong>Auditoria:</strong> Todos os acessos e operações são registrados em logs imutáveis.',
        '<strong>Backup:</strong> Cópias de segurança diárias com armazenamento redundante em locais geograficamente distribuídos.',
        '<strong>Monitoramento:</strong> Monitoramento contínuo de ameaças e vulnerabilidades.',
        '<strong>Atualização:</strong> Aplicação regular de patches de segurança e atualizações.',
        '<strong>Treinamento:</strong> Capacitação contínua da equipe em boas práticas de segurança e proteção de dados.',
        'Apesar de todos os esforços, nenhum sistema é totalmente imune a riscos. Em caso de incidente de segurança, os titulares afetados serão notificados conforme determinado pela LGPD.'
      ]
    },
    {
      id: 'direitos',
      title: 'Direitos dos Titulares',
      icon: 'verified_user',
      content: [
        'Conforme a LGPD, você tem os seguintes direitos sobre seus dados pessoais:',
        '<strong>Confirmação e Acesso:</strong> Confirmar se há tratamento de dados e acessar seus dados.',
        '<strong>Correção:</strong> Solicitar a correção de dados incompletos, inexatos ou desatualizados.',
        '<strong>Anonimização, Bloqueio ou Eliminação:</strong> Requerer a anonimização, bloqueio ou eliminação de dados desnecessários, excessivos ou tratados em desconformidade.',
        '<strong>Portabilidade:</strong> Solicitar a portabilidade dos dados a outro fornecedor de serviço.',
        '<strong>Eliminação:</strong> Requerer a eliminação dos dados tratados com consentimento.',
        '<strong>Informação:</strong> Obter informação sobre entidades públicas e privadas com as quais compartilhamos dados.',
        '<strong>Informação sobre Consentimento:</strong> Ser informado sobre a possibilidade de não fornecer consentimento e sobre as consequências.',
        '<strong>Revogação do Consentimento:</strong> Revogar o consentimento a qualquer momento.',
        'Para exercer esses direitos, entre em contato através do e-mail: dpo@age.mg.gov.br'
      ]
    },
    {
      id: 'retencao',
      title: 'Retenção de Dados',
      icon: 'schedule',
      content: [
        'Os dados pessoais são retidos pelo tempo necessário para cumprir as finalidades para as quais foram coletados, incluindo:',
        '<strong>Dados de Assinatura:</strong> Mantidos permanentemente para fins de validade jurídica e auditoria.',
        '<strong>Logs de Auditoria:</strong> Retidos por 5 anos para fins de compliance e investigação.',
        '<strong>Dados de Cadastro:</strong> Mantidos enquanto a conta estiver ativa ou conforme exigido por lei.',
        '<strong>Dados de Navegação:</strong> Retidos por 6 meses.',
        'Após o período de retenção, os dados são eliminados de forma segura ou anonimizados para fins estatísticos.',
        'Dados essenciais podem ser mantidos por períodos superiores quando exigido por lei ou para exercício regular de direitos.'
      ]
    },
    {
      id: 'cookies',
      title: 'Cookies e Tecnologias Similares',
      icon: 'cookie',
      content: [
        'O SignatureHub utiliza cookies e tecnologias similares para:',
        '<strong>Cookies Essenciais:</strong> Necessários para o funcionamento básico da plataforma, como autenticação e preferências.',
        '<strong>Cookies de Desempenho:</strong> Coletam informações sobre como os usuários utilizam o site para melhorias.',
        '<strong>Cookies de Funcionalidade:</strong> Lembram escolhas do usuário para personalizar a experiência.',
        'Você pode gerenciar suas preferências de cookies através das configurações do seu navegador. No entanto, a desativação de cookies essenciais pode afetar a funcionalidade do sistema.',
        'Para mais informações, consulte nossa Política de Cookies (link disponível no rodapé).'
      ]
    },
    {
      id: 'menores',
      title: 'Dados de Menores',
      icon: 'child_care',
      content: [
        'O SignatureHub não coleta intencionalmente dados de menores de 18 anos.',
        'Caso seja identificado que dados de menores foram coletados inadvertidamente, tomaremos medidas imediatas para excluí-los de nossos registros.',
        'Se você acredita que um menor forneceu dados pessoais ao SignatureHub, entre em contato conosco imediatamente através do e-mail: desenvolvimento@age.mg.gov.br'
      ]
    },
    {
      id: 'alteracoes',
      title: 'Alterações nesta Política',
      icon: 'update',
      content: [
        'Esta Política de Privacidade pode ser atualizada periodicamente para refletir mudanças em nossas práticas de tratamento de dados ou alterações legais.',
        'Alterações significativas serão comunicadas através de:',
        '• Notificação por e-mail aos usuários cadastrados',
        '• Aviso destacado no sistema',
        '• Atualização da data de "Última Atualização" no topo desta página',
        'Recomendamos que você revise esta política regularmente para se manter informado sobre como protegemos seus dados.',
        'O uso continuado do SignatureHub após alterações constitui aceitação da política atualizada.'
      ]
    },
    {
      id: 'contato',
      title: 'Fale Conosco',
      icon: 'contact_mail',
      content: [
        'Se você tiver dúvidas, comentários ou solicitações relacionadas a esta Política de Privacidade, entre em contato:',
        '<strong>E-mail:</strong> desenvolvimento@age.mg.gov.br',
        '<strong>Endereço:</strong> Avenida Afonso Pena, 4.000, Bairro Cruzeiro - Belo Horizonte – Minas Gerais',
        'Responderemos todas as solicitações no prazo máximo de 15 dias úteis, conforme estabelecido pela LGPD.'
      ]
    }
  ];

  ngOnInit(): void {}

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      const offset = 100;
      const elementPosition = element.getBoundingClientRect().top;
      const offsetPosition = elementPosition + window.pageYOffset - offset;

      window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
      });
    }
  }

  printPolicy(): void {
    window.print();
  }

  downloadPolicy(): void {
    alert('Funcionalidade de download em desenvolvimento');
  }
}
